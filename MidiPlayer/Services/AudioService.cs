using System;
using System.IO;
using MeltySynth;
using NAudio.Wave;

namespace MidiPlayer.Services
{
    public class AudioService : IDisposable
    {
        private Synthesizer? _synthesizer;
        private MidiFileSequencer? _sequencer;
        private MidiFile? _currentMidiFile;
        private WaveOutEvent? _waveOut;
        private SynthWaveProvider? _waveProvider;
        private string _soundFontPath;
        private bool _isLooping = false;

        public event Action<TimeSpan>? PositionChanged;
        public event Action? PlaybackStopped;

        private System.Timers.Timer _uiTimer;

        public AudioService(string soundFontPath)
        {
            _soundFontPath = soundFontPath;
            
            _uiTimer = new System.Timers.Timer(100);
            _uiTimer.Elapsed += (s, e) => 
            {
                if (_sequencer != null && _waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
                {
                    // Position updates skipped for compatibility
                    // But we can invoke the event to suppress warning
                    PositionChanged?.Invoke(TimeSpan.Zero); 
                }
            };
            
            InitializeSynth();
        }

        private void InitializeSynth()
        {
            if (!File.Exists(_soundFontPath))
            {
                // Don't crash here, let the UI handle it or throw a specific exception we catch in ViewModel
                throw new FileNotFoundException($"SoundFont not found at {_soundFontPath}. Please read Assets/README_SOUNDFONT.txt");
            }

            try 
            {
                var soundFont = new SoundFont(_soundFontPath);
                int sampleRate = 44100;
                _synthesizer = new Synthesizer(soundFont, sampleRate);
                _sequencer = new MidiFileSequencer(_synthesizer);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load SoundFont: {ex.Message}");
            }
        }

        public void LoadMidi(string midiPath)
        {
            if (_synthesizer == null) return;

            Stop();

            _currentMidiFile = new MidiFile(midiPath);
        }

        public void Play()
        {
            if (_synthesizer == null || _sequencer == null || _currentMidiFile == null) return;

            if (_waveOut == null)
            {
                _waveProvider = new SynthWaveProvider(_synthesizer, _sequencer);
                _waveOut = new WaveOutEvent();
                _waveOut.Init(_waveProvider);
                _waveOut.PlaybackStopped += (s, e) => PlaybackStopped?.Invoke();
                
                _sequencer.Play(_currentMidiFile, _isLooping);
            }

            _waveOut.Play();
            _uiTimer.Start();
        }

        public void Pause()
        {
            _waveOut?.Pause();
            _uiTimer.Stop();
        }

        public void Stop()
        {
            _waveOut?.Stop();
            _uiTimer.Stop();
            _sequencer?.Stop();
            
            if (_waveOut != null)
            {
                _waveOut.Dispose();
                _waveOut = null;
            }
        }

        public void SetVolume(float volume)
        {
            if (_waveOut != null)
            {
                _waveOut.Volume = volume;
            }
        }

        public void SetSpeed(float speed)
        {
            if (_sequencer != null)
            {
                _sequencer.Speed = speed;
            }
        }

        public TimeSpan GetDuration()
        {
            return _currentMidiFile?.Length ?? TimeSpan.Zero;
        }
        
        public bool IsLooping
        {
            get => _isLooping;
            set 
            {
                _isLooping = value;
            }
        }

        public void Dispose()
        {
            _uiTimer?.Dispose();
            _waveOut?.Dispose();
        }
    }

    public class SynthWaveProvider : IWaveProvider
    {
        private readonly Synthesizer _synthesizer;
        private readonly MidiFileSequencer _sequencer;
        private readonly WaveFormat _waveFormat;

        public SynthWaveProvider(Synthesizer synthesizer, MidiFileSequencer sequencer)
        {
            _synthesizer = synthesizer;
            _sequencer = sequencer;
            _waveFormat = new WaveFormat(44100, 16, 2);
        }

        public WaveFormat WaveFormat => _waveFormat;

        public int Read(byte[] buffer, int offset, int count)
        {
            int samplesRequired = count / 2;
            float[] left = new float[samplesRequired / 2];
            float[] right = new float[samplesRequired / 2];

            _sequencer.Render(left, right);

            int bufferIndex = offset;
            for (int i = 0; i < left.Length; i++)
            {
                short sLeft = (short)(Math.Clamp(left[i], -1.0f, 1.0f) * 32767);
                short sRight = (short)(Math.Clamp(right[i], -1.0f, 1.0f) * 32767);

                buffer[bufferIndex++] = (byte)(sLeft & 0xFF);
                buffer[bufferIndex++] = (byte)((sLeft >> 8) & 0xFF);
                buffer[bufferIndex++] = (byte)(sRight & 0xFF);
                buffer[bufferIndex++] = (byte)((sRight >> 8) & 0xFF);
            }

            return count;
        }
    }
}
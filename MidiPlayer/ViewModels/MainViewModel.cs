using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MidiPlayer.Models;
using MidiPlayer.Services;
using MidiPlayer.Views;
using Newtonsoft.Json;
using ReactiveUI;

namespace MidiPlayer.ViewModels
{
    public class MainViewModel : ReactiveObject, IDisposable
    {
        private AudioService? _audioService;
        private string _statusText = "Ready";
        private string _currentTime = "00:00";
        private string _totalTime = "00:00";
        private double _volume = 1.0;
        private float _tempo = 1.0f;
        private bool _isLooping = false;
        private MidiTrackItem? _selectedTrack;
        private bool _isPlaying = false;

        public ObservableCollection<MidiTrackItem> Playlist { get; } = new();

        public string StatusText
        {
            get => _statusText;
            set => this.RaiseAndSetIfChanged(ref _statusText, value);
        }

        public string CurrentTime
        {
            get => _currentTime;
            set => this.RaiseAndSetIfChanged(ref _currentTime, value);
        }

        public string TotalTime
        {
            get => _totalTime;
            set => this.RaiseAndSetIfChanged(ref _totalTime, value);
        }

        public double Volume
        {
            get => _volume;
            set
            {
                this.RaiseAndSetIfChanged(ref _volume, value);
                _audioService?.SetVolume((float)value);
            }
        }

        public float Tempo
        {
            get => _tempo;
            set
            {
                this.RaiseAndSetIfChanged(ref _tempo, value);
                _audioService?.SetSpeed(value);
            }
        }

        public bool IsLooping
        {
            get => _isLooping;
            set
            {
                this.RaiseAndSetIfChanged(ref _isLooping, value);
                if(_audioService != null) _audioService.IsLooping = value;
            }
        }

        public MidiTrackItem? SelectedTrack
        {
            get => _selectedTrack;
            set => this.RaiseAndSetIfChanged(ref _selectedTrack, value);
        }

        public ReactiveCommand<Unit, Unit> PlayCommand { get; }
        public ReactiveCommand<Unit, Unit> PauseCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
        public ReactiveCommand<Unit, Unit> AddFilesCommand { get; }
        public ReactiveCommand<Unit, Unit> SavePlaylistCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadPlaylistCommand { get; }

        public MainViewModel()
        {
            // Initialize commands
            PlayCommand = ReactiveCommand.Create(Play);
            PauseCommand = ReactiveCommand.Create(Pause);
            StopCommand = ReactiveCommand.Create(Stop);
            AddFilesCommand = ReactiveCommand.CreateFromTask(AddFiles);
            SavePlaylistCommand = ReactiveCommand.CreateFromTask(SavePlaylist);
            LoadPlaylistCommand = ReactiveCommand.CreateFromTask(LoadPlaylist);

            InitializeAudio();
            LoadSampleFiles();
        }

        private void InitializeAudio()
        {
            try
            {
                string sfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "TimGM6mb.sf2");
                
                // Fallback check
                if (!File.Exists(sfPath))
                {
                    // Try to look in project structure if debugging
                    string debugPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "TimGM6mb.sf2");
                    if (File.Exists(debugPath)) sfPath = debugPath;
                }

                if (File.Exists(sfPath))
                {
                    _audioService = new AudioService(sfPath);
                    _audioService.PositionChanged += OnPositionChanged;
                    _audioService.PlaybackStopped += OnPlaybackStopped;
                    StatusText = "Audio Engine Loaded. SoundFont Ready.";
                }
                else
                {
                    StatusText = "Error: SoundFont TimGM6mb.sf2 not found in Assets.";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Audio Error: {ex.Message}";
            }
        }

        private void LoadSampleFiles()
        {
            try 
            {
                // Check in AppDomain BaseDirectory first (where executable is)
                var samplesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples");
                
                // If not found, try project level (for debugging purpose)
                if (!Directory.Exists(samplesPath))
                {
                    samplesPath = Path.Combine(Directory.GetCurrentDirectory(), "Samples");
                }

                if (Directory.Exists(samplesPath))
                {
                    var files = Directory.GetFiles(samplesPath, "*.mid");
                    foreach (var file in files)
                    {
                        // Check if already in playlist to avoid duplicates if re-initialized
                        if (!Playlist.Any(p => p.FilePath == file))
                        {
                            Playlist.Add(new MidiTrackItem
                            {
                                FileName = Path.GetFileName(file),
                                FilePath = file,
                                Duration = TimeSpan.Zero
                            });
                        }
                    }

                    if (Playlist.Count > 0 && SelectedTrack == null)
                    {
                        SelectedTrack = Playlist[0];
                        StatusText = $"Loaded {Playlist.Count} sample tracks.";
                    }
                }
            }
            catch (Exception ex)
            {
                // Silent fail or log
                Console.WriteLine($"Error loading samples: {ex.Message}");
            }
        }

        private void OnPositionChanged(TimeSpan pos)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                CurrentTime = pos.ToString(@"mm\:ss");
            });
        }

        private void OnPlaybackStopped()
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _isPlaying = false;
                StatusText = "Stopped";
                // Auto play next logic could be added here
                if(SelectedTrack != null && Playlist.Count > 1)
                {
                     // Simple auto next
                     // var index = Playlist.IndexOf(SelectedTrack);
                     // if(index < Playlist.Count - 1) { SelectedTrack = Playlist[index+1]; Play(); }
                }
            });
        }

        private void Play()
        {
            if (_audioService == null) 
            {
                 StatusText = "Audio Engine not ready. Check SoundFont.";
                 return;
            }

            if(SelectedTrack == null) return;

            // Simple check: if file doesn't exist (e.g. sample not downloaded correctly)
            if(!File.Exists(SelectedTrack.FilePath))
            {
                StatusText = $"File not found: {SelectedTrack.FileName}";
                return;
            }

            try
            {
                if (!_isPlaying)
                {
                     // Force reload or load new
                    _audioService.LoadMidi(SelectedTrack.FilePath);
                    var dur = _audioService.GetDuration();
                    TotalTime = dur.ToString(@"mm\:ss");
                }
                
                _audioService.Play();
                _isPlaying = true;
                StatusText = $"Playing: {SelectedTrack.FileName}";
            }
            catch(Exception ex)
            {
                StatusText = $"Playback Error: {ex.Message}";
                _isPlaying = false;
            }
        }

        private void Pause()
        {
            _audioService?.Pause();
            StatusText = "Paused";
        }

        private void Stop()
        {
            _audioService?.Stop();
            _isPlaying = false;
            CurrentTime = "00:00";
            StatusText = "Stopped";
        }

        private async Task AddFiles()
        {
            var window = (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null) return;

            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Midi Files",
                AllowMultiple = true,
                FileTypeFilter = new[] { new FilePickerFileType("MIDI Files") { Patterns = new[] { "*.mid", "*.midi" } } }
            });

            foreach (var file in files)
            {
                var path = file.Path.LocalPath;
                Playlist.Add(new MidiTrackItem
                {
                    FileName = file.Name,
                    FilePath = path,
                    Duration = TimeSpan.Zero 
                });
            }
            
            if (Playlist.Count > 0 && SelectedTrack == null)
            {
                SelectedTrack = Playlist[0];
            }
        }

        private async Task SavePlaylist()
        {
            var window = (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null) return;

            var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Playlist",
                DefaultExtension = ".json",
                FileTypeChoices = new[] { new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } } }
            });

            if (file != null)
            {
                var json = JsonConvert.SerializeObject(Playlist, Formatting.Indented);
                await File.WriteAllTextAsync(file.Path.LocalPath, json);
                StatusText = "Playlist saved.";
            }
        }

        private async Task LoadPlaylist()
        {
             var window = (Avalonia.Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (window == null) return;

            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Load Playlist",
                AllowMultiple = false,
                FileTypeFilter = new[] { new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } } }
            });

            if (files.Count > 0)
            {
                var path = files[0].Path.LocalPath;
                var json = await File.ReadAllTextAsync(path);
                var items = JsonConvert.DeserializeObject<ObservableCollection<MidiTrackItem>>(json);
                if (items != null)
                {
                    Playlist.Clear();
                    foreach (var item in items) Playlist.Add(item);
                }
                StatusText = "Playlist loaded.";
            }
        }

        public void Dispose()
        {
            _audioService?.Dispose();
        }
    }
}
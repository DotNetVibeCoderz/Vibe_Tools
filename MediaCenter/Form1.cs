using System;
using System.Drawing;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace MediaCenter
{
    public partial class Form1 : Form
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private bool _isDraggingSeek = false;
        private string _currentVisual = "";
        private string _lastUri = "";
        private bool _isLastNetwork = false;

        public Form1()
        {
            InitializeComponent();
            KeyPreview = true; // For Hotkeys
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Core Initialization
            Core.Initialize();
            
            InitializeLibVLC(new string[0]);
        }

        private void InitializeLibVLC(string[] args)
        {
            // Save current state if player exists
            long savedTime = 0;
            int savedVolume = trackBarVolume.Value;
            bool wasPlaying = false;
            Media savedMedia = null;

            if (_mediaPlayer != null)
            {
                wasPlaying = _mediaPlayer.IsPlaying;
                if(wasPlaying) savedTime = _mediaPlayer.Time;
                savedVolume = _mediaPlayer.Volume;
                savedMedia = _mediaPlayer.Media;
                
                _mediaPlayer.Stop();
                _mediaPlayer.Dispose();
            }
            if (_libVLC != null)
            {
                _libVLC.Dispose();
            }

            // Create new LibVLC with arguments
            // We add some default hints to ensure video output works for visualizations
            var finalArgs = new List<string>(args);
            // finalArgs.Add("--vout=direct3d11"); // Optional: Force a video backend if needed

            _libVLC = new LibVLC(enableDebugLogs: true, options: finalArgs.ToArray());
            _mediaPlayer = new MediaPlayer(_libVLC);
            
            videoView1.MediaPlayer = _mediaPlayer;

            // Event Subscriptions
            _mediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            _mediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
            _mediaPlayer.EndReached += MediaPlayer_EndReached;
            
            // Restore Volume
            _mediaPlayer.Volume = savedVolume;
        }

        #region Media Player Events

        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                // Reset UI
                trackBarSeek.Value = 0;
                lblTime.Text = "00:00";
            }));
        }

        private void MediaPlayer_LengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                lblDuration.Text = TimeSpan.FromMilliseconds(e.Length).ToString(@"mm\:ss");
            }));
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            if (_isDraggingSeek) return;

            this.Invoke(new Action(() =>
            {
                // Update Time Label
                lblTime.Text = TimeSpan.FromMilliseconds(e.Time).ToString(@"mm\:ss");

                // Update Trackbar
                long length = _mediaPlayer.Length;
                if (length > 0)
                {
                    double position = (double)e.Time / length;
                    int val = (int)(position * trackBarSeek.Maximum);
                    if (val >= trackBarSeek.Minimum && val <= trackBarSeek.Maximum)
                    {
                        trackBarSeek.Value = val;
                    }
                }
            }));
        }

        #endregion

        #region Controls

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (_mediaPlayer.Media != null)
            {
                _mediaPlayer.Play();
            }
            else if (!string.IsNullOrEmpty(_lastUri))
            {
                 PlayMedia(_lastUri, _isLastNetwork);
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (_mediaPlayer.CanPause)
            {
                _mediaPlayer.Pause();
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _mediaPlayer.Stop();
            trackBarSeek.Value = 0;
            lblTime.Text = "00:00";
        }

        private void trackBarVolume_Scroll(object sender, EventArgs e)
        {
            _mediaPlayer.Volume = trackBarVolume.Value;
        }

        private void trackBarSeek_MouseDown(object sender, MouseEventArgs e)
        {
            _isDraggingSeek = true;
        }

        private void trackBarSeek_MouseUp(object sender, MouseEventArgs e)
        {
            if (_mediaPlayer.IsPlaying && _mediaPlayer.Length > 0)
            {
                float position = (float)trackBarSeek.Value / trackBarSeek.Maximum;
                _mediaPlayer.Position = position;
            }
            _isDraggingSeek = false;
        }
        
        private void trackBarSeek_Scroll(object sender, EventArgs e)
        {
             if(_isDraggingSeek && _mediaPlayer.Length > 0)
             {
                 long time = (long)(_mediaPlayer.Length * ((float)trackBarSeek.Value / trackBarSeek.Maximum));
                 lblTime.Text = TimeSpan.FromMilliseconds(time).ToString(@"mm\:ss");
             }
        }

        #endregion

        #region Menu Items

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Media Files|*.mp4;*.mkv;*.avi;*.mp3;*.wav;*.flac;*.mov;*.wmv|All Files|*.*";
                ofd.Title = "Select Media";
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    PlayMedia(ofd.FileName);
                }
            }
        }

        private void openNetworkStreamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string url = SimpleInputBox("Enter Stream URL (YouTube, HLS, Radio):", "Open Stream");
            if (!string.IsNullOrWhiteSpace(url))
            {
                PlayMedia(url, true);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        
        // Visualization Selection
        private void visNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateVisualization("");
        }

        private void visSpectrumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateVisualization("spectrum");
        }

        private void visScopeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateVisualization("scope");
        }
        
        private void UpdateVisualization(string vis)
        {
             _currentVisual = vis;
             
             // Update Menu Check State
             visNoneToolStripMenuItem.Checked = (vis == "");
             visSpectrumToolStripMenuItem.Checked = (vis == "spectrum");
             visScopeToolStripMenuItem.Checked = (vis == "scope");

             // Build LibVLC arguments
             // To use visualizations, we typically enable the 'visual' audio filter 
             // and specify the effect.
             var args = new List<string>();
             
             if (!string.IsNullOrEmpty(vis))
             {
                 args.Add("--audio-visual=visual");
                 args.Add($"--effect-list={vis}");
                 // Force visualizer to try and use the video output
                 // args.Add("--vout=any"); 
             }

             // Restart LibVLC with new args
             InitializeLibVLC(args.ToArray());
             
             // Resume playback if we had a file
             if (!string.IsNullOrEmpty(_lastUri))
             {
                 PlayMedia(_lastUri, _isLastNetwork);
             }
        }

        // Tools
        private void takeSnapshotToolStripMenuItem_Click(object sender, EventArgs e)
        {
             if(_mediaPlayer.IsPlaying)
             {
                 string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), $"Snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                 // Ensure the file path ends with .png or .jpg, LibVLC detects format by extension
                 bool result = _mediaPlayer.TakeSnapshot(0, path, 0, 0);
                 if(result) MessageBox.Show($"Snapshot saved to {path}");
                 else MessageBox.Show("Failed to take snapshot. Make sure a video is playing.");
             }
        }

        private void mediaInformationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_mediaPlayer.Media != null)
            {
                // We parse explicitly to ensure meta is available
                // _mediaPlayer.Media.Parse(MediaParseOptions.ParseNetwork); 
                // Note: Parsing might be async, but often basic tags are available after load
                
                var info = $"Title: {_mediaPlayer.Media.Meta(MetadataType.Title)}\n" +
                           $"Artist: {_mediaPlayer.Media.Meta(MetadataType.Artist)}\n" +
                           $"Album: {_mediaPlayer.Media.Meta(MetadataType.Album)}\n" +
                           $"Duration: {TimeSpan.FromMilliseconds(_mediaPlayer.Media.Duration).ToString()}\n" +
                           $"Type: {_mediaPlayer.Media.Type}";
                MessageBox.Show(info, "Media Information");
            }
        }

        #endregion
        
        #region Hotkeys
        
        // Handle keyboard shortcuts
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Play/Pause - Space
            if (keyData == Keys.Space)
            {
                if(_mediaPlayer.IsPlaying && _mediaPlayer.CanPause) _mediaPlayer.Pause();
                else _mediaPlayer.Play();
                return true;
            }
            
            // Stop - Escape
            if (keyData == Keys.Escape)
            {
                btnStop.PerformClick();
                return true;
            }

            // Seek Forward - Right Arrow (Small step) or Ctrl+Right (Large step)
            if (keyData == Keys.Right || keyData == (Keys.Control | Keys.Right))
            {
                if(_mediaPlayer.IsPlaying)
                {
                    var newTime = _mediaPlayer.Time + 10000; // +10s
                    if (newTime > _mediaPlayer.Length) newTime = _mediaPlayer.Length;
                    _mediaPlayer.Time = newTime;
                    return true;
                }
            }
            
            // Seek Backward - Left Arrow or Ctrl+Left
             if (keyData == Keys.Left || keyData == (Keys.Control | Keys.Left))
            {
                if(_mediaPlayer.IsPlaying)
                {
                    var newTime = _mediaPlayer.Time - 10000; // -10s
                    if (newTime < 0) newTime = 0;
                    _mediaPlayer.Time = newTime;
                    return true;
                }
            }
            
            // Volume Up - Up Arrow
            if (keyData == Keys.Up)
            {
                 int newVol = _mediaPlayer.Volume + 5;
                 if(newVol > 100) newVol = 100;
                 _mediaPlayer.Volume = newVol;
                 trackBarVolume.Value = newVol;
                 return true;
            }
            
            // Volume Down - Down Arrow
            if (keyData == Keys.Down)
            {
                 int newVol = _mediaPlayer.Volume - 5;
                 if(newVol < 0) newVol = 0;
                 _mediaPlayer.Volume = newVol;
                 trackBarVolume.Value = newVol;
                 return true;
            }
            
            // Snapshot - Ctrl+S
            if (keyData == (Keys.Control | Keys.S))
            {
                takeSnapshotToolStripMenuItem_Click(this, EventArgs.Empty);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
        
        #endregion

        private void PlayMedia(string uri, bool isNetwork = false)
        {
            try
            {
                _lastUri = uri;
                _isLastNetwork = isNetwork;
                
                var media = new Media(_libVLC, uri, isNetwork ? FromType.FromLocation : FromType.FromPath);
                
                // Parse media for meta (Async is better but sync is ok for simple needs)
                media.Parse(MediaParseOptions.ParseNetwork);

                _mediaPlayer.Play(media);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading media: " + ex.Message);
            }
        }

        // Helper for InputBox
        private string SimpleInputBox(string prompt, string title)
        {
            Form promptForm = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = prompt, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 80, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { promptForm.Close(); };
            
            promptForm.Controls.Add(textBox);
            promptForm.Controls.Add(confirmation);
            promptForm.Controls.Add(textLabel);
            promptForm.AcceptButton = confirmation;

            return promptForm.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
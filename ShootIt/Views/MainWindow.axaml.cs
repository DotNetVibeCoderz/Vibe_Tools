using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ShootIt.Services;
using System;
using System.Threading.Tasks;

namespace ShootIt.Views
{
    public partial class MainWindow : Window
    {
        private readonly CaptureService _captureService;
        private readonly RecorderService _recorderService;
        private bool _isRecording = false;

        public MainWindow()
        {
            InitializeComponent();
            _captureService = new CaptureService();
            _recorderService = new RecorderService();

            var btnFull = this.FindControl<Button>("BtnFullScreen");
            var btnRegion = this.FindControl<Button>("BtnRegion");
            var btnRecord = this.FindControl<Button>("BtnRecord");

            btnFull.Click += async (s, e) => await CaptureFull();
            btnRegion.Click += (s, e) => StartRegionCapture();
            btnRecord.Click += (s, e) => ToggleRecording();
        }

        private async Task CaptureFull()
        {
            SetStatus("Capturing in 1 second...");
            this.WindowState = WindowState.Minimized;
            await Task.Delay(1000); // Give time to minimize

            string file = _captureService.CaptureFullScreen();
            
            this.WindowState = WindowState.Normal;
            if (file != null)
                SetStatus($"Saved: {System.IO.Path.GetFileName(file)}");
            else
                SetStatus("Capture failed.");
        }

        private void StartRegionCapture()
        {
            this.WindowState = WindowState.Minimized;
            var regionWindow = new RegionSelector();
            regionWindow.RegionSelected += (rect) =>
            {
                string file = _captureService.CaptureRegion((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
                this.WindowState = WindowState.Normal;
                
                if (file != null)
                    SetStatus($"Region Saved: {System.IO.Path.GetFileName(file)}");
                else
                    SetStatus("Region capture canceled or failed.");
            };
            regionWindow.Show();
        }

        private void ToggleRecording()
        {
            var btnRecord = this.FindControl<Button>("BtnRecord");

            if (!_isRecording)
            {
                // Start
                this.WindowState = WindowState.Minimized;
                string file = _recorderService.StartRecording();
                _isRecording = true;
                
                btnRecord.Content = "⏹ Stop Recording";
                SetStatus("Recording... (Check 'Recordings' folder)");
            }
            else
            {
                // Stop
                _recorderService.StopRecording();
                _isRecording = false;
                
                this.WindowState = WindowState.Normal;
                btnRecord.Content = "🎥 Start Recording";
                SetStatus("Recording Stopped & Saved.");
            }
        }

        private void SetStatus(string msg)
        {
            Dispatcher.UIThread.InvokeAsync(() => {
                var txt = this.FindControl<TextBlock>("StatusText");
                if (txt != null) txt.Text = msg;
            });
        }
    }
}
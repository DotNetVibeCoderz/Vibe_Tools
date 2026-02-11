using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using LPRNet.Database;
using LPRNet.Models;
using LPRNet.Services;
using OpenCvSharp;
using ReactiveUI;

namespace LPRNet.ViewModels
{
    public class MainViewModel : ReactiveObject, IDisposable
    {
        private readonly LprContext _dbContext;
        private readonly ILprService _lprService;

        private string _statusMessage = "Ready";
        private ObservableCollection<PlateRecord> _logs;
        private Bitmap? _previewImage;
        private string _searchQuery = "";
        
        // Video processing fields
        private VideoCapture? _videoCapture;
        private DispatcherTimer? _videoTimer;
        private bool _isVideoPlaying;
        private int _frameCount = 0;

        public MainViewModel()
        {
            _dbContext = new LprContext();
            _dbContext.Database.EnsureCreated();

            _lprService = new SimpleLprService();
            Logs = new ObservableCollection<PlateRecord>();

            CaptureCommand = ReactiveCommand.CreateFromTask(CaptureImageAsync);
            SearchCommand = ReactiveCommand.CreateFromTask(SearchLogsAsync);
            ClearLogsCommand = ReactiveCommand.CreateFromTask(ClearLogsAsync);
            StopVideoCommand = ReactiveCommand.Create(StopVideo);

            _ = LoadLogsAsync();
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public ObservableCollection<PlateRecord> Logs
        {
            get => _logs;
            set => this.RaiseAndSetIfChanged(ref _logs, value);
        }

        public Bitmap? PreviewImage
        {
            get => _previewImage;
            set => this.RaiseAndSetIfChanged(ref _previewImage, value);
        }
        
        public bool IsVideoPlaying
        {
            get => _isVideoPlaying;
            set => this.RaiseAndSetIfChanged(ref _isVideoPlaying, value);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set => this.RaiseAndSetIfChanged(ref _searchQuery, value);
        }

        public ICommand CaptureCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearLogsCommand { get; }
        public ICommand StopVideoCommand { get; }

        public async Task ProcessFileAsync(string filePath)
        {
            StopVideo();

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                StatusMessage = "File not found.";
                return;
            }

            string ext = Path.GetExtension(filePath).ToLower();
            string[] videoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".wmv" };

            if (videoExtensions.Contains(ext))
            {
                StartVideo(filePath);
            }
            else
            {
                await ProcessSingleImageAsync(filePath);
            }
        }

        private void StartVideo(string filePath)
        {
            try
            {
                _videoCapture = new VideoCapture(filePath);
                if (!_videoCapture.IsOpened())
                {
                    StatusMessage = "Failed to open video.";
                    return;
                }

                IsVideoPlaying = true;
                StatusMessage = "Playing video...";
                _frameCount = 0;

                _videoTimer = new DispatcherTimer();
                _videoTimer.Interval = TimeSpan.FromMilliseconds(33); // ~30 FPS
                _videoTimer.Tick += VideoTimer_Tick;
                _videoTimer.Start();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error starting video: {ex.Message}";
                StopVideo();
            }
        }

        private void VideoTimer_Tick(object? sender, EventArgs e)
        {
            if (_videoCapture == null || !_videoCapture.IsOpened())
            {
                StopVideo();
                return;
            }

            using (var frame = new Mat())
            {
                if (!_videoCapture.Read(frame) || frame.Empty())
                {
                    StatusMessage = "Video finished.";
                    StopVideo();
                    return;
                }

                // Update Preview
                UpdatePreviewFromMat(frame);

                // Process LPR every 30 frames (approx once per second) to optimize performance
                _frameCount++;
                if (_frameCount % 30 == 0)
                {
                    // Run LPR in background to not block UI
                    // We need to clone the frame because 'frame' will be disposed at end of using block
                    // but the Task might run later. 
                    // However, to keep it simple and thread-safe for OpenCvSharp, we save to temp file or memory.
                    // For this example, let's use a temp file approach or modify LprService to accept byte[].
                    // Since LprService accepts path, we save a temp frame.
                    
                    var tempFramePath = Path.Combine(Path.GetTempPath(), "lpr_video_frame.jpg");
                    frame.SaveImage(tempFramePath);
                    
                    // Fire and forget (or await properly if we want to ensure sequence)
                    _ = ProcessLprForFrame(tempFramePath); 
                }
            }
        }

        private async Task ProcessLprForFrame(string imagePath)
        {
            try
            {
                var result = await _lprService.ProcessImageAsync(imagePath);
                if (result != null && result.PlateNumber != "UNREADABLE")
                {
                    // Invoke on UI thread to update collection
                    Dispatcher.UIThread.Post(() =>
                    {
                        AddLog(result);
                        StatusMessage = $"Detected: {result.PlateNumber}";
                    });
                }
            }
            catch { /* Ignore errors during video stream processing */ }
        }

        private void StopVideo()
        {
            _videoTimer?.Stop();
            _videoTimer = null;
            _videoCapture?.Dispose();
            _videoCapture = null;
            IsVideoPlaying = false;
        }

        private async Task ProcessSingleImageAsync(string filePath)
        {
            StatusMessage = "Processing image...";
            
            // Display image
            using (var stream = File.OpenRead(filePath))
            {
                PreviewImage = new Bitmap(stream);
            }

            var result = await _lprService.ProcessImageAsync(filePath);
            if (result != null)
            {
                AddLog(result);
                StatusMessage = $"Plate Detected: {result.PlateNumber} ({result.Confidence:P0})";
            }
            else
            {
                StatusMessage = "No plate detected.";
            }
        }

        private void UpdatePreviewFromMat(Mat mat)
        {
            // Convert OpenCvSharp Mat to Avalonia Bitmap
            // Efficient way: Encode to memory stream as BMP/JPG and load
            using (var ms = mat.ToMemoryStream(".bmp"))
            {
                ms.Position = 0;
                PreviewImage = new Bitmap(ms);
            }
        }

        private async Task CaptureImageAsync()
        {
            // Existing Simulation Logic
            StatusMessage = "Simulating Capture...";
            try
            {
                string tempFile = Path.Combine(Path.GetTempPath(), $"plate_{DateTime.Now.Ticks}.jpg");
                await File.WriteAllBytesAsync(tempFile, Array.Empty<byte>()); // The service handles the dummy file logic or fails gracefully
                
                // In a real scenario, this would capture from a camera device. 
                // Since the user asked to support 'File', 'Video', 'Mockup', 
                // we keep this as the 'Mockup/Simulation' or 'Camera' placeholder.
                
                // If the service actually needs a real image, we might need to copy a sample image here
                // or ensure LprService generates one.
                
                var result = await _lprService.ProcessImageAsync(tempFile);
                if (result != null)
                {
                    //load simulated image
                    if (File.Exists(tempFile))
                    {
                        using (var stream = File.OpenRead(tempFile))
                        {
                            PreviewImage = new Bitmap(stream);
                        }
                    }
                    AddLog(result);
                    StatusMessage = $"Simulation Result: {result.PlateNumber}";
                }
                else
                {
                    StatusMessage = "Simulation: No result.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Capture failed: {ex.Message}";
            }
        }

        private void AddLog(PlateRecord record)
        {
            _dbContext.AddRecordAsync(record).ConfigureAwait(false); 
            // Note: DB is async but we don't await here to avoid blocking UI in high freq video
            
            Logs.Insert(0, record);
        }

        private async Task LoadLogsAsync()
        {
            try
            {
                var records = await _dbContext.SearchByPlateAsync(string.Empty);
                Logs = new ObservableCollection<PlateRecord>(records);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load logs: {ex.Message}";
            }
        }

        private async Task SearchLogsAsync()
        {
            try
            {
                var records = await _dbContext.SearchByPlateAsync(SearchQuery);
                Logs = new ObservableCollection<PlateRecord>(records);
                StatusMessage = $"Found {records.Count} records.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Search failed: {ex.Message}";
            }
        }

        private async Task ClearLogsAsync()
        {
            try 
            {
                await _dbContext.ClearAllRecordsAsync();
                Logs.Clear();
                StatusMessage = "Logs cleared.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to clear logs: {ex.Message}";
            }
        }

        public void Dispose()
        {
            StopVideo();
        }
    }
}
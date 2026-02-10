using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using ObjectCounter.Models;
using ObjectCounter.Services;
using ObjectCounter.Views;
using OpenCvSharp;
using ReactiveUI;

namespace ObjectCounter.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private object _currentView;
        private Bitmap? _currentFrame;
        private bool _isUrlVisible;
        private bool _isBrowseVisible;
        private int _sourceTypeIndex;
        private string _sourceUrl = "";
        private bool _showBoxes = true;
        private bool _showLabels = true;
        private int _totalCount;
        private string _fps = "0";
        private bool _isRunning;
        private CancellationTokenSource? _cts;

        private YoloDetector? _detector;
        private ObjectTracker _tracker; // Removed readonly to allow reset
        private readonly DatabaseService _dbService;
        private readonly ApiServer _apiServer;

        public ObservableCollection<LogEntry> LogEntries { get; } = new ObservableCollection<LogEntry>();

        public Func<bool, Task<string?>>? OpenFileDialogHandler { get; set; }

        public MainWindowViewModel()
        {
            _dbService = new DatabaseService();
            _apiServer = new ApiServer(_dbService);
            _tracker = new ObjectTracker();
            
            try
            {
                string modelPath = "yolov8n.onnx";
                if (File.Exists(modelPath))
                {
                    _detector = new YoloDetector(modelPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error init detector: {ex.Message}");
            }

            ShowSourceCommand = ReactiveCommand.Create(() => SwitchView(new SourceView { DataContext = this }));
            ShowStatsCommand = ReactiveCommand.Create(() => {
                LoadLogs();
                SwitchView(new StatsView { DataContext = this });
            });
            ShowAboutCommand = ReactiveCommand.Create(() => SwitchView(new AboutView()));
            ResetCommand = ReactiveCommand.Create(ResetApp);
            StartCommand = ReactiveCommand.Create(StartCapture);
            StopCommand = ReactiveCommand.Create(StopCapture);
            BrowseCommand = ReactiveCommand.CreateFromTask(BrowseFile);

            CurrentView = new SourceView { DataContext = this };
            
            _apiServer.Start();
        }

        public object CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }

        public Bitmap? CurrentFrame
        {
            get => _currentFrame;
            set => this.RaiseAndSetIfChanged(ref _currentFrame, value);
        }

        public int SourceTypeIndex
        {
            get => _sourceTypeIndex;
            set
            {
                this.RaiseAndSetIfChanged(ref _sourceTypeIndex, value);
                // 0: Webcam
                // 1: Image File
                // 2: Video File
                // 3: IP Camera
                IsUrlVisible = value != 0;
                IsBrowseVisible = value == 1 || value == 2;
            }
        }

        public bool IsUrlVisible
        {
            get => _isUrlVisible;
            set => this.RaiseAndSetIfChanged(ref _isUrlVisible, value);
        }

        public bool IsBrowseVisible
        {
            get => _isBrowseVisible;
            set => this.RaiseAndSetIfChanged(ref _isBrowseVisible, value);
        }

        public string SourceUrl
        {
            get => _sourceUrl;
            set => this.RaiseAndSetIfChanged(ref _sourceUrl, value);
        }

        public bool ShowBoxes
        {
            get => _showBoxes;
            set => this.RaiseAndSetIfChanged(ref _showBoxes, value);
        }

        public bool ShowLabels
        {
            get => _showLabels;
            set => this.RaiseAndSetIfChanged(ref _showLabels, value);
        }

        public int TotalCount
        {
            get => _totalCount;
            set => this.RaiseAndSetIfChanged(ref _totalCount, value);
        }

        public string Fps
        {
            get => _fps;
            set => this.RaiseAndSetIfChanged(ref _fps, value);
        }

        public ReactiveCommand<Unit, Unit> ShowSourceCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowStatsCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowAboutCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetCommand { get; }
        public ReactiveCommand<Unit, Unit> StartCommand { get; }
        public ReactiveCommand<Unit, Unit> StopCommand { get; }
        public ReactiveCommand<Unit, Unit> BrowseCommand { get; }

        private void SwitchView(object view)
        {
            CurrentView = view;
        }

        private async Task BrowseFile()
        {
            if (OpenFileDialogHandler != null)
            {
                bool isImage = SourceTypeIndex == 1;
                var path = await OpenFileDialogHandler(isImage);
                if (!string.IsNullOrEmpty(path))
                {
                    SourceUrl = path;
                }
            }
        }

        private void ResetApp()
        {
            StopCapture();
            _dbService.ClearLogs();
            LogEntries.Clear();
            TotalCount = 0;
            Fps = "0";
            // Reset Tracker
            _tracker = new ObjectTracker();
        }

        private void LoadLogs()
        {
            var logs = _dbService.GetLogs(100);
            LogEntries.Clear();
            foreach (var log in logs)
            {
                LogEntries.Add(log);
            }
        }

        private void StartCapture()
        {
            if (_isRunning) return;
            
            // Re-initialize tracker on start to ensure fresh state if needed, 
            // or we can keep it if we want persistent tracking across pauses.
            // For now, let's keep it unless Reset is pressed, or if TotalCount is 0 (fresh start).
            if (TotalCount == 0)
            {
                _tracker = new ObjectTracker();
            }

            _isRunning = true;
            _cts = new CancellationTokenSource();

            int deviceIndex = 0; 
            string filename = SourceUrl;

            Task.Run(() => CaptureLoop(deviceIndex, filename, _cts.Token));
        }

        private void StopCapture()
        {
            _isRunning = false;
            _cts?.Cancel();
        }

        private void CaptureLoop(int deviceIndex, string filename, CancellationToken token)
        {
            VideoCapture? capture = null;
            try
            {
                if (SourceTypeIndex == 1) // Image File
                {
                    if (File.Exists(filename))
                    {
                        using var imgMat = Cv2.ImRead(filename);
                        if (!imgMat.Empty())
                        {
                            // Reset tracker for single image to ensure it detects
                            _tracker = new ObjectTracker();
                            // Reset count UI for this run? No, we might want cumulative.
                            // But usually image detection is one-shot.
                            // Let's just process.
                            
                            ProcessAndDisplay(imgMat);
                            while (_isRunning && !token.IsCancellationRequested)
                            {
                                Thread.Sleep(100);
                            }
                        }
                    }
                    return;
                }

                // Video / Webcam / IP Camera
                if (SourceTypeIndex == 0)
                {
                    capture = new VideoCapture(deviceIndex);
                }
                else
                {
                    capture = new VideoCapture(filename);
                }

                if (!capture.IsOpened())
                {
                    Console.WriteLine("Failed to open source");
                    _isRunning = false;
                    return;
                }

                using var frameMat = new Mat();
                var lastTime = DateTime.Now;
                int frameCount = 0;

                while (_isRunning && !token.IsCancellationRequested)
                {
                    capture.Read(frameMat);
                    if (frameMat.Empty()) break;

                    ProcessAndDisplay(frameMat);

                    // FPS Update
                    frameCount++;
                    if ((DateTime.Now - lastTime).TotalSeconds >= 1)
                    {
                        string fpsStr = frameCount.ToString();
                        Dispatcher.UIThread.Post(() => Fps = fpsStr);
                        frameCount = 0;
                        lastTime = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Capture Error: {ex.Message}");
            }
            finally
            {
                capture?.Release();
                capture?.Dispose();
                _isRunning = false;
            }
        }

        private void ProcessAndDisplay(Mat mat)
        {
            if (_detector != null)
            {
                var detections = _detector.Detect(mat);
                detections = _tracker.Update(detections);

                if (detections.Any())
                {
                    // Use a local variable for thread-safe comparison with the UI property logic
                    // We assume TotalCount tracks the max TrackId seen + 1.
                    
                    int maxId = detections.Max(d => d.TrackId);
                    int currentTotal = _totalCount; // Access field directly

                    if (maxId + 1 > currentTotal)
                    {
                        // Identify new objects: those with TrackId >= currentTotal
                        var newObjects = detections.Where(d => d.TrackId >= currentTotal).ToList();
                        
                        string sourceName = SourceTypeIndex switch
                        {
                            1 => "Image",
                            2 => "Video",
                            3 => "IP Camera",
                            _ => "Webcam"
                        };

                        foreach (var obj in newObjects)
                        {
                            // Log to database
                            _dbService.LogCount(obj.Label, 1, sourceName);
                            Console.WriteLine($"Logged: {obj.Label} ID:{obj.TrackId}");
                        }

                        int newTotal = maxId + 1;
                        Dispatcher.UIThread.Post(() => TotalCount = newTotal);
                    }
                }

                foreach (var d in detections)
                {
                    if (ShowBoxes)
                    {
                        Cv2.Rectangle(mat, new Rect((int)d.X, (int)d.Y, (int)d.Width, (int)d.Height), Scalar.Red, 2);
                    }
                    if (ShowLabels)
                    {
                        Cv2.PutText(mat, $"{d.Label} #{d.TrackId} {d.Confidence:F2}", new Point(d.X, d.Y - 5), HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow, 2);
                    }
                }
            }

            var bitmap = MatToBitmap(mat);
            Dispatcher.UIThread.Post(() => CurrentFrame = bitmap);
        }

        private Bitmap MatToBitmap(Mat mat)
        {
            Cv2.ImEncode(".png", mat, out byte[] buf);
            using var ms = new MemoryStream(buf);
            return new Bitmap(ms);
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageExplorer.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageExplorer.ViewModels
{
    public partial class BatchConvertViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;

        public event Action? RequestClose;

        [ObservableProperty]
        private string _sourcePath = string.Empty;

        [ObservableProperty]
        private string _destPath = string.Empty;

        public ObservableCollection<string> Actions { get; } = new ObservableCollection<string>
        {
            "Rotate", "Convert Format", "Resize"
        };

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRotateVisible))]
        [NotifyPropertyChangedFor(nameof(IsConvertVisible))]
        [NotifyPropertyChangedFor(nameof(IsResizeVisible))]
        [NotifyPropertyChangedFor(nameof(IsParametersVisible))]
        private string _selectedAction = "Rotate";

        public bool IsRotateVisible => SelectedAction == "Rotate";
        public bool IsConvertVisible => SelectedAction == "Convert Format";
        public bool IsResizeVisible => SelectedAction == "Resize";
        public bool IsParametersVisible => !string.IsNullOrEmpty(SelectedAction);

        // Parameters
        [ObservableProperty] private double _rotationDegrees = 90;
        
        public ObservableCollection<string> TargetFormats { get; } = new ObservableCollection<string> { ".png", ".jpg", ".webp", ".bmp" };
        
        [ObservableProperty] private string _targetExtension = ".png";
        
        [ObservableProperty] private decimal _resizeWidth = 800;
        [ObservableProperty] private decimal _resizeHeight = 600;

        // Progress
        [ObservableProperty] private int _totalFiles = 100;
        [ObservableProperty] private int _processedFiles = 0;
        [ObservableProperty] private string _statusMessage = "Ready";
        [ObservableProperty] private bool _isBusy = false;

        public BatchConvertViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        [RelayCommand]
        public async Task BrowseSource()
        {
            var path = await _dialogService.OpenFolderDialogAsync();
            if (path != null) SourcePath = path;
        }

        [RelayCommand]
        public async Task BrowseDest()
        {
            var path = await _dialogService.OpenFolderDialogAsync();
            if (path != null) DestPath = path;
        }

        [RelayCommand]
        public void Close()
        {
            RequestClose?.Invoke();
        }

        [RelayCommand]
        public async Task Start()
        {
            if (string.IsNullOrEmpty(SourcePath) || string.IsNullOrEmpty(DestPath))
            {
                StatusMessage = "Please select source and destination folders.";
                return;
            }

            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var files = Directory.GetFiles(SourcePath)
                    .Where(f => new[] { ".jpg", ".jpeg", ".png", ".webp", ".bmp" }.Contains(Path.GetExtension(f).ToLower()))
                    .ToList();

                TotalFiles = files.Count;
                ProcessedFiles = 0;
                StatusMessage = $"Found {TotalFiles} images. Processing...";

                BatchImageProcessor.BatchAction actionType = BatchImageProcessor.BatchAction.Rotate;
                object param = null;

                switch (SelectedAction)
                {
                    case "Rotate":
                        actionType = BatchImageProcessor.BatchAction.Rotate;
                        param = (float)RotationDegrees;
                        break;
                    case "Convert Format":
                        actionType = BatchImageProcessor.BatchAction.Convert;
                        param = TargetExtension;
                        break;
                    case "Resize":
                        actionType = BatchImageProcessor.BatchAction.Resize;
                        param = new Tuple<int, int>((int)ResizeWidth, (int)ResizeHeight);
                        break;
                }

                await Task.Run(() =>
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            BatchImageProcessor.ProcessImage(file, DestPath, actionType, param);
                        }
                        catch (Exception ex)
                        {
                            // Log error? For now just skip
                            System.Diagnostics.Debug.WriteLine($"Failed to process {file}: {ex.Message}");
                        }
                        
                        // Update UI on main thread
                        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                        {
                            ProcessedFiles++;
                            StatusMessage = $"Processed {ProcessedFiles}/{TotalFiles}";
                        });
                    }
                });

                StatusMessage = "Batch processing complete!";
                await _dialogService.ShowMessageAsync("Success", $"Processed {ProcessedFiles} files successfully.");
            }
            catch (Exception ex)
            {
                StatusMessage = "Error occurred.";
                await _dialogService.ShowMessageAsync("Error", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
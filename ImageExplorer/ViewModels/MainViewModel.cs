using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageExplorer.Models;
using ImageExplorer.Services;
using System.Collections.Generic;

namespace ImageExplorer.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private IDialogService? _dialogService;
        
        [ObservableProperty]
        private ObservableCollection<DirectoryNode> _directories = new();

        [ObservableProperty]
        private DirectoryNode? _selectedDirectory;

        [ObservableProperty]
        private ObservableCollection<ImageItem> _files = new();

        [ObservableProperty]
        private ImageItem? _selectedFile;

        [ObservableProperty]
        private Bitmap? _currentImage;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private string _selectedTool = "None";
        
        [ObservableProperty]
        private Color _brushColor = Colors.Red;

        [ObservableProperty]
        private double _brushThickness = 2.0;

        [ObservableProperty]
        private double _zoomScale = 1.0;

        // Colors
        public List<NamedColor> AvailableColors { get; } = new()
        {
            new NamedColor { Name = "Red", Color = Colors.Red },
            new NamedColor { Name = "Green", Color = Colors.Green },
            new NamedColor { Name = "Blue", Color = Colors.Blue },
            new NamedColor { Name = "Yellow", Color = Colors.Yellow },
            new NamedColor { Name = "Black", Color = Colors.Black },
            new NamedColor { Name = "White", Color = Colors.White },
            new NamedColor { Name = "Orange", Color = Colors.Orange },
            new NamedColor { Name = "Purple", Color = Colors.Purple },
            new NamedColor { Name = "Cyan", Color = Colors.Cyan },
            new NamedColor { Name = "Magenta", Color = Colors.Magenta },
            new NamedColor { Name = "Gray", Color = Colors.Gray }
        };

        [ObservableProperty]
        private NamedColor _selectedColorOption;

        partial void OnSelectedColorOptionChanged(NamedColor value)
        {
             if (value != null)
             {
                 BrushColor = value.Color;
             }
        }

        // Store logical annotations here
        public ObservableCollection<AnnotationItem> Annotations { get; } = new();

        // Event to tell View to clear UI canvas
        public event EventHandler? ClearAnnotationsRequested;

        public MainViewModel()
        {
            LoadDrives();
            SelectedColorOption = AvailableColors.FirstOrDefault() ?? new NamedColor { Name = "Red", Color = Colors.Red };
        }
        
        public void SetDialogService(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        private void LoadDrives()
        {
            Directories.Clear();
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                var node = new DirectoryNode(drive.Name);
                node.LoadChildren(); 
                Directories.Add(node);
            }
        }

        partial void OnSelectedDirectoryChanged(DirectoryNode? value)
        {
            if (value == null) return;
            LoadFiles(value.Path);
            value.LoadChildren();
            foreach (var child in value.Children)
            {
                child.LoadChildren();
            }
        }

        private void LoadFiles(string path)
        {
            try
            {
                Files.Clear();
                var dirInfo = new DirectoryInfo(path);
                var extensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp" };
                
                foreach (var file in dirInfo.GetFiles())
                {
                    if (extensions.Contains(file.Extension.ToLower()))
                    {
                        Files.Add(new ImageItem
                        {
                            FilePath = file.FullName,
                            FileName = file.Name,
                            Size = file.Length,
                            DateModified = file.LastWriteTime
                        });
                    }
                }
                StatusMessage = $"Loaded {Files.Count} images from {path}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading files: {ex.Message}";
            }
        }

        partial void OnSelectedFileChanged(ImageItem? value)
        {
            if (value == null) 
            {
                CurrentImage = null;
                return;
            }
            LoadImage(value.FilePath);
        }

        private void LoadImage(string path)
        {
            try
            {
                // Clear previous annotations
                Annotations.Clear();
                ClearAnnotationsRequested?.Invoke(this, EventArgs.Empty);

                using (var stream = File.OpenRead(path))
                {
                    CurrentImage = new Bitmap(stream);
                }
                StatusMessage = $"Viewing {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading image: {ex.Message}";
            }
        }
        
        [RelayCommand]
        public async Task OpenFile()
        {
            if (_dialogService == null) return;
            var path = await _dialogService.OpenFileDialogAsync();
            if (!string.IsNullOrEmpty(path))
            {
                 SelectedFile = null;
                 LoadImage(path);
                 StatusMessage = $"Opened external file: {path}";
            }
        }
        
        [RelayCommand]
        public async Task SaveFile()
        {
            if (_dialogService == null || CurrentImage == null) return;
            
            var defaultName = SelectedFile?.FileName ?? "image.png";
            var path = await _dialogService.SaveFileDialogAsync(defaultName);
            
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    // If we have annotations, apply them to a temporary bitmap first
                    Bitmap bitmapToSave = CurrentImage;
                    
                    if (Annotations.Count > 0)
                    {
                        var annotated = ImageProcessor.ApplyAnnotations(CurrentImage, Annotations);
                        if (annotated != null)
                        {
                            bitmapToSave = annotated;
                        }
                    }

                    bitmapToSave.Save(path);
                    StatusMessage = $"Saved to {path}";
                    
                    if (bitmapToSave != CurrentImage)
                    {
                         bitmapToSave.Dispose(); // Clean up temp bitmap
                    }
                }
                catch (Exception ex)
                {
                    await _dialogService.ShowMessageAsync("Error", $"Failed to save: {ex.Message}");
                }
            }
        }

        [RelayCommand]
        public async Task RotateImage()
        {
             if (_dialogService == null || CurrentImage == null) return;
             
             // Commit annotations before rotate? For now, we clear them as they won't align
             // In a perfect world we rotate the annotation coordinates too.
             if (Annotations.Count > 0)
             {
                 await _dialogService.ShowMessageAsync("Rotate", "Flattening annotations for rotation.");
                 // Simple dialog service doesn't support bool return yet in interface, assuming yes or just flattening.
                 // Let's flatten them automatically for better UX
                 var annotated = ImageProcessor.ApplyAnnotations(CurrentImage, Annotations);
                 if (annotated != null)
                 {
                     CurrentImage = annotated;
                     Annotations.Clear();
                     ClearAnnotationsRequested?.Invoke(this, EventArgs.Empty);
                 }
             }

             var result = await _dialogService.ShowRotateDialogAsync();
             if (result.HasValue)
             {
                 var processed = ImageProcessor.Rotate(CurrentImage, result.Value);
                 if (processed != null)
                 {
                     CurrentImage = processed;
                     StatusMessage = $"Rotated by {result.Value} degrees";
                 }
             }
        }

        [RelayCommand]
        public async Task ResizeImage()
        {
             if (_dialogService == null || CurrentImage == null) return;
             
             if (Annotations.Count > 0)
             {
                 // Flatten
                 var annotated = ImageProcessor.ApplyAnnotations(CurrentImage, Annotations);
                 if (annotated != null)
                 {
                     CurrentImage = annotated;
                     Annotations.Clear();
                     ClearAnnotationsRequested?.Invoke(this, EventArgs.Empty);
                 }
             }

             var currentSize = CurrentImage.PixelSize;
             var result = await _dialogService.ShowResizeDialogAsync(currentSize.Width, currentSize.Height);
             
             if (result.HasValue)
             {
                 var processed = ImageProcessor.Resize(CurrentImage, result.Value.Width, result.Value.Height);
                 if (processed != null)
                 {
                     CurrentImage = processed;
                     StatusMessage = $"Resized to {result.Value.Width}x{result.Value.Height}";
                 }
             }
        }
        
        [RelayCommand]
        public void ApplyFilter(string filterName)
        {
             if (CurrentImage == null) return;

             // Flatten annotations first so filter applies to them too? Or keep them on top?
             // Usually filters apply to the image. Annotations are vector layers on top.
             // But if we want to "Grayscale" everything including red lines, we flatten.
             // Let's keep them separate for now unless user saves.
             
             var processed = ImageProcessor.ApplyFilter(CurrentImage, filterName);
             if (processed != null)
             {
                 CurrentImage = processed;
                 StatusMessage = $"Applied filter: {filterName}";
             }
        }

        [RelayCommand]
        public void SelectTool(string toolName)
        {
            SelectedTool = toolName;
            StatusMessage = $"Tool Selected: {toolName}";
        }

        [RelayCommand]
        public async Task BatchConvert()
        {
            if (_dialogService != null)
            {
                await _dialogService.ShowBatchConvertDialogAsync();
            }
        }

        [RelayCommand]
        public void ZoomIn()
        {
            ZoomScale = Math.Min(ZoomScale * 1.2, 10.0); // Max zoom 10x
            StatusMessage = $"Zoom: {ZoomScale:P0}";
        }

        [RelayCommand]
        public void ZoomOut()
        {
            ZoomScale = Math.Max(ZoomScale / 1.2, 0.1); // Min zoom 10%
            StatusMessage = $"Zoom: {ZoomScale:P0}";
        }
        
        [RelayCommand]
        public void ResetZoom()
        {
            ZoomScale = 1.0;
            StatusMessage = "Zoom: 100%";
        }
    }
}
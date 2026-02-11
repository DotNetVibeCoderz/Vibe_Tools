using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using LPRNet.ViewModels;
using System.Linq;

namespace LPRNet.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async void OnOpenFileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure we can access the TopLevel for storage provider
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return;

                var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Open Image or Video for LPR",
                    AllowMultiple = false,
                    FileTypeFilter = new[]
                    {
                        new FilePickerFileType("Media Files") 
                        { 
                            Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.mp4", "*.avi", "*.mkv", "*.mov", "*.wmv" } 
                        },
                        FilePickerFileTypes.ImageAll,
                        new FilePickerFileType("Videos") 
                        { 
                            Patterns = new[] { "*.mp4", "*.avi", "*.mkv", "*.mov", "*.wmv" } 
                        }
                    }
                });

                if (files.Count > 0)
                {
                    var file = files[0];
                    // Get the file path. In Avalonia 11, Path is a Uri. 
                    // LocalPath handles the 'file://' prefix removal if present.
                    var filePath = file.Path.LocalPath;

                    if (DataContext is MainViewModel vm)
                    {
                        await vm.ProcessFileAsync(filePath);
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Fallback for older systems or unexpected errors
                var dialog = new OpenFileDialog();
                dialog.Filters.Add(new FileDialogFilter { Name = "Images & Videos", Extensions = { "jpg", "png", "mp4", "avi" } });
                var result = await dialog.ShowAsync(this);
                
                if (result != null && result.Length > 0 && DataContext is MainViewModel vm)
                {
                    await vm.ProcessFileAsync(result[0]);
                }
            }
        }
    }
}
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using ObjectCounter.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ObjectCounter.Views
{
    public partial class SourceView : UserControl
    {
        public SourceView()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is MainWindowViewModel vm)
            {
                vm.OpenFileDialogHandler = ShowOpenFileDialog;
            }
        }

        private async Task<string?> ShowOpenFileDialog(bool isImage)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return null;

            var options = new FilePickerOpenOptions
            {
                Title = isImage ? "Open Image File" : "Open Video File",
                AllowMultiple = false
            };

            if (isImage)
            {
                options.FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Images") { Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp" } },
                };
            }
            else
            {
                options.FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Videos") { Patterns = new[] { "*.mp4", "*.avi", "*.mkv", "*.mov" } },
                };
            }

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
            return files.Count > 0 ? files[0].Path.LocalPath : null;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

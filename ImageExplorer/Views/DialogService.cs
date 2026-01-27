using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using ImageExplorer.Services;
using ImageExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageExplorer.Views
{
    public class DialogService : IDialogService
    {
        private readonly Window _owner;

        public DialogService(Window owner)
        {
            _owner = owner;
        }

        public async Task<string?> OpenFileDialogAsync()
        {
            var files = await _owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Image",
                AllowMultiple = false,
                FileTypeFilter = new List<FilePickerFileType>
                {
                    new FilePickerFileType("Images")
                    {
                        Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.webp" }
                    }
                }
            });

            return files.Count > 0 ? files[0].Path.LocalPath : null;
        }

        public async Task<string?> OpenFolderDialogAsync()
        {
            var folders = await _owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Folder",
                AllowMultiple = false
            });

            return folders.Count > 0 ? folders[0].Path.LocalPath : null;
        }

        public async Task<string?> SaveFileDialogAsync(string defaultName)
        {
            var file = await _owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Image",
                DefaultExtension = "png",
                SuggestedFileName = defaultName,
                 FileTypeChoices = new List<FilePickerFileType>
                {
                    new FilePickerFileType("PNG Image") { Patterns = new[] { "*.png" } },
                    new FilePickerFileType("JPEG Image") { Patterns = new[] { "*.jpg" } }
                }
            });

            return file?.Path.LocalPath;
        }

        public async Task<(int Width, int Height)?> ShowResizeDialogAsync(int currentWidth, int currentHeight)
        {
            var dialog = new Window
            {
                Title = "Resize Image",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var widthBox = new TextBox { Text = currentWidth.ToString(), Margin = new Thickness(0, 5, 0, 5) };
            var heightBox = new TextBox { Text = currentHeight.ToString(), Margin = new Thickness(0, 5, 0, 5) };
            var okBtn = new Button { Content = "OK", HorizontalAlignment = HorizontalAlignment.Right, Width = 80 };
            var cancelBtn = new Button { Content = "Cancel", HorizontalAlignment = HorizontalAlignment.Left, Width = 80 };

            var panel = new StackPanel { Margin = new Thickness(20) };
            panel.Children.Add(new TextBlock { Text = "Width:" });
            panel.Children.Add(widthBox);
            panel.Children.Add(new TextBlock { Text = "Height:" });
            panel.Children.Add(heightBox);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 20, Margin = new Thickness(0, 20, 0, 0), HorizontalAlignment = HorizontalAlignment.Center };
            btnPanel.Children.Add(cancelBtn);
            btnPanel.Children.Add(okBtn);
            panel.Children.Add(btnPanel);

            dialog.Content = panel;

            bool confirmed = false;
            okBtn.Click += (_, _) => { confirmed = true; dialog.Close(); };
            cancelBtn.Click += (_, _) => dialog.Close();

            await dialog.ShowDialog(_owner);

            if (confirmed && int.TryParse(widthBox.Text, out int w) && int.TryParse(heightBox.Text, out int h))
            {
                return (w, h);
            }
            return null;
        }

        public async Task<float?> ShowRotateDialogAsync()
        {
            var dialog = new Window
            {
                Title = "Rotate Image",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            var rotateBox = new TextBox { Text = "90", Margin = new Thickness(0, 5, 0, 5) };
            var combo = new ComboBox { Width = 200, Margin = new Thickness(0, 5, 0, 10) };
            combo.ItemsSource = new[] { "90", "180", "270" };
            combo.SelectedItem = "90";
            combo.SelectionChanged += (s, e) => { if(combo.SelectedItem != null) rotateBox.Text = combo.SelectedItem.ToString(); };

            var okBtn = new Button { Content = "Rotate", HorizontalAlignment = HorizontalAlignment.Right, Width = 80 };
            var cancelBtn = new Button { Content = "Cancel", HorizontalAlignment = HorizontalAlignment.Left, Width = 80 };

            var panel = new StackPanel { Margin = new Thickness(20) };
            panel.Children.Add(new TextBlock { Text = "Degrees:" });
            panel.Children.Add(rotateBox);
            panel.Children.Add(new TextBlock { Text = "Presets:" });
            panel.Children.Add(combo);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 20, Margin = new Thickness(0, 20, 0, 0), HorizontalAlignment = HorizontalAlignment.Center };
            btnPanel.Children.Add(cancelBtn);
            btnPanel.Children.Add(okBtn);
            panel.Children.Add(btnPanel);

            dialog.Content = panel;

            bool confirmed = false;
            okBtn.Click += (_, _) => { confirmed = true; dialog.Close(); };
            cancelBtn.Click += (_, _) => dialog.Close();

            await dialog.ShowDialog(_owner);

            if (confirmed && float.TryParse(rotateBox.Text, out float r))
            {
                return r;
            }
            return null;
        }

        public async Task ShowBatchConvertDialogAsync()
        {
            var vm = new BatchConvertViewModel(this);
            var dialog = new BatchConvertWindow
            {
                DataContext = vm
            };
            
            // Allow the ViewModel to close the window
            vm.RequestClose += () => dialog.Close();
            
            await dialog.ShowDialog(_owner);
        }
        
        public async Task ShowMessageAsync(string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };
            
            var panel = new StackPanel { Margin = new Thickness(20), Spacing = 10, VerticalAlignment = VerticalAlignment.Center };
            panel.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, HorizontalAlignment = HorizontalAlignment.Center });
            var btn = new Button { Content = "OK", HorizontalAlignment = HorizontalAlignment.Center };
            btn.Click += (_, _) => dialog.Close();
            panel.Children.Add(btn);
            
            dialog.Content = panel;
            await dialog.ShowDialog(_owner);
        }
    }
}
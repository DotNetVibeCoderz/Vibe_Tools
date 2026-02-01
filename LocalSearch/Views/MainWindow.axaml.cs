using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using LocalSearch.Models;
using LocalSearch.ViewModels;
using System;

namespace LocalSearch.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnResultDoubleTapped(object? sender, TappedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid != null && grid.SelectedItem is SearchResultItem item)
            {
                var vm = DataContext as MainWindowViewModel;
                vm?.OpenResultCommand.Execute(item);
            }
        }

        private async void OnBrowseClick(object? sender, RoutedEventArgs e)
        {
            try 
            {
                // Mengambil akses ke sistem window/storage
                var topLevel = TopLevel.GetTopLevel(this);
                if (topLevel == null) return;

                // Membuka dialog pilih folder
                var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select Folder to Index",
                    AllowMultiple = false
                });

                // Jika user memilih folder
                if (folders.Count > 0)
                {
                    var selectedFolder = folders[0];
                    var vm = DataContext as MainWindowViewModel;
                    if (vm != null)
                    {
                        // Mengambil path lokal dari URI
                        vm.RootPath = selectedFolder.Path.LocalPath;
                    }
                }
            }
            catch (Exception ex)
            {
                // Safety net kalau ada error permission dll, meskipun jarang terjadi di dialog
                System.Diagnostics.Debug.WriteLine($"Error picking folder: {ex.Message}");
            }
        }
    }
}

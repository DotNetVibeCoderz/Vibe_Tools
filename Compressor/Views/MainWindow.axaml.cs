using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Compressor.ViewModels;
using Compressor.Models;
using System.Threading.Tasks;
using System.Linq;

namespace Compressor.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnDataContextChanged(System.EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is MainWindowViewModel vm)
            {
                vm.ShowCompressDialog = async (path) =>
                {
                    var dlg = new CompressDialog(path);
                    await dlg.ShowDialog(this);
                    return (dlg.IsConfirmed, dlg.DestinationPath, dlg.Format);
                };

                vm.ShowExtractDialog = async (path) =>
                {
                    var dlg = new ExtractDialog(path);
                    await dlg.ShowDialog(this);
                    return (dlg.IsConfirmed, dlg.DestinationPath);
                };

                vm.ShowConfirmDialog = async (message) =>
                {
                    var dlg = new ConfirmDialog(message);
                    await dlg.ShowDialog(this);
                    return dlg.IsConfirmed;
                };

                vm.ShowRenameDialog = async (currentName) =>
                {
                    var dlg = new RenameDialog(currentName);
                    await dlg.ShowDialog(this);
                    return (dlg.IsConfirmed, dlg.NewName);
                };
            }
        }

        private void OnDoubleTapped(object? sender, TappedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm && vm.SelectedItem != null)
            {
                vm.OpenItem(vm.SelectedItem);
            }
        }

        private void OnTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // Explicitly handle selection change to ensure ViewModel gets updated
            if (DataContext is MainWindowViewModel vm)
            {
                var selectedItem = e.AddedItems.Cast<object>().FirstOrDefault();
                if (selectedItem is DirectoryNode node)
                {
                    vm.SelectedFolder = node;
                }
            }
        }

        private void OnExitClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void OnAboutClick(object? sender, RoutedEventArgs e)
        {
            var dialog = new Window
            {
                Width = 300,
                Height = 150,
                Title = "About",
                Content = new TextBlock
                {
                    Text = "Compressor App\nBuilt with Avalonia UI\nBy Jacky the Code Bender",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    TextAlignment = Avalonia.Media.TextAlignment.Center
                },
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            await dialog.ShowDialog(this);
        }
    }
}
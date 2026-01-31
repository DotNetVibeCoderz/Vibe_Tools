using Avalonia.Controls;
using Avalonia.Interactivity;
using StockInfo.ViewModels;

namespace StockInfo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void OnLoadStockClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.AddOrSelectStock();
        }
    }
}

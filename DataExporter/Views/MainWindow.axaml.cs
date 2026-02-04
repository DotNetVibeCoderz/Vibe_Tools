using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DataExporter.ViewModels;

namespace DataExporter.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

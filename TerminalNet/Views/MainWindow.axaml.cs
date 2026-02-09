using Avalonia.Controls;
using TerminalNet.ViewModels;

namespace TerminalNet.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
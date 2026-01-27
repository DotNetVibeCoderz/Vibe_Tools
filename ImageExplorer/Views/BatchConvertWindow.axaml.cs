using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ImageExplorer.ViewModels;

namespace ImageExplorer.Views
{
    public partial class BatchConvertWindow : Window
    {
        public BatchConvertWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
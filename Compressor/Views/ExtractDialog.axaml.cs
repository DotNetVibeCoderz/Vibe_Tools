using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Compressor.Views
{
    public partial class ExtractDialog : Window
    {
        public string DestinationPath => PathBox.Text ?? "";
        public bool IsConfirmed { get; private set; } = false;

        public ExtractDialog()
        {
            InitializeComponent();
        }

        public ExtractDialog(string defaultPath) : this()
        {
            PathBox.Text = defaultPath;
        }

        private void OnExtractClick(object? sender, RoutedEventArgs e)
        {
             if (string.IsNullOrWhiteSpace(PathBox.Text)) return;
             IsConfirmed = true;
             Close();
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Compressor.Views
{
    public partial class CompressDialog : Window
    {
        public string DestinationPath => PathBox.Text ?? "";
        public string Format => (FormatBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Zip";
        public bool IsConfirmed { get; private set; } = false;

        public CompressDialog()
        {
            InitializeComponent();
        }

        public CompressDialog(string defaultPath) : this()
        {
            PathBox.Text = defaultPath;
        }

        private void OnCompressClick(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PathBox.Text))
            {
                // Simple validation
                return;
            }
            IsConfirmed = true;
            Close();
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
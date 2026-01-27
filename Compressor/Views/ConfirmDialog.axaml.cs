using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Compressor.Views
{
    public partial class ConfirmDialog : Window
    {
        public bool IsConfirmed { get; private set; } = false;

        public ConfirmDialog()
        {
            InitializeComponent();
        }

        public ConfirmDialog(string message, string title = "Confirmation") : this()
        {
            this.Title = title;
            this.FindControl<TextBlock>("MessageText")!.Text = message;
        }

        private void Ok_Click(object? sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            Close();
        }

        private void Cancel_Click(object? sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            Close();
        }
    }
}
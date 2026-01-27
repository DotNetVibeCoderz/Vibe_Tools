using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Compressor.Views
{
    public partial class RenameDialog : Window
    {
        public bool IsConfirmed { get; private set; } = false;
        public string NewName => this.FindControl<TextBox>("NameTextBox")?.Text ?? "";

        public RenameDialog()
        {
            InitializeComponent();
        }

        public RenameDialog(string currentName) : this()
        {
            var box = this.FindControl<TextBox>("NameTextBox");
            if (box != null)
            {
                box.Text = currentName;
                box.SelectAll();
            }
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
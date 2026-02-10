using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ObjectCounter.Views
{
    public partial class StatsView : UserControl
    {
        public StatsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

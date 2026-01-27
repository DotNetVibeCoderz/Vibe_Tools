using Avalonia.Media;

namespace ImageExplorer.Models
{
    public class NamedColor
    {
        public string Name { get; set; } = string.Empty;
        public Color Color { get; set; }
        public IBrush Brush => new SolidColorBrush(Color);
    }
}
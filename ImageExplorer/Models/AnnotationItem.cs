using Avalonia;
using Avalonia.Media;
using System.Collections.Generic;

namespace ImageExplorer.Models
{
    public enum AnnotationType
    {
        Line,
        Rectangle,
        Ellipse,
        Point, // Old single dot
        Pen // Freehand drawing
    }

    public class AnnotationItem
    {
        public AnnotationType Type { get; set; }
        public Color Color { get; set; }
        public double Thickness { get; set; }
        
        // Coordinates in Original Image Pixels
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        
        // For Lines
        public double EndX { get; set; }
        public double EndY { get; set; }

        // For Pen (Freehand)
        public List<Point> Points { get; set; } = new List<Point>();
    }
}
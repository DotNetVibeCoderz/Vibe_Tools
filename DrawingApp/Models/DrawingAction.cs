using Avalonia;
using Avalonia.Media;
using System.Collections.Generic;

namespace DrawingApp.Models;

/// <summary>
/// Representasi dari satu aksi menggambar - untuk undo/redo
/// </summary>
public class DrawingAction
{
    public DrawingTool Tool { get; set; }
    public List<Point> Points { get; set; } = new List<Point>();
    public Color Color { get; set; }
    public double StrokeThickness { get; set; }
    public BrushStyle BrushStyle { get; set; }

    // Text properties
    public string? Text { get; set; }
    public double FontSize { get; set; }
    public string? FontFamilyName { get; set; }
}

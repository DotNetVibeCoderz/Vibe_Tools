using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using DrawingApp.Models;
using System;
using System.Collections.Generic;

namespace DrawingApp.Controls;

/// <summary>
/// Custom control untuk canvas menggambar - Jantung aplikasi!
/// </summary>
public class DrawingCanvas : Control
{
    private List<DrawingAction> _actions = new();
    private DrawingAction? _currentAction;
    private bool _isDrawing = false;
    
    // Property untuk menyimpan background image yang di-load
    private Bitmap? _backgroundImage;
    
    public DrawingTool CurrentTool { get; set; } = DrawingTool.Pencil;
    public Color CurrentColor { get; set; } = Colors.Black;
    public double StrokeThickness { get; set; } = 2;
    public BrushStyle BrushStyle { get; set; } = BrushStyle.Normal;
    public bool ShowGrid { get; set; } = false;

    // Text settings (diisi dari MainWindow)
    public string CurrentText { get; set; } = "";
    public string CurrentFontFamily { get; set; } = "Arial";
    public double CurrentFontSize { get; set; } = 24;

    /// <summary>
    /// Load gambar dari file dan set sebagai background canvas
    /// </summary>
    public void LoadImage(Bitmap image)
    {
        _backgroundImage = image;
        
        // Resize canvas sesuai ukuran gambar
        Width = image.PixelSize.Width;
        Height = image.PixelSize.Height;
        
        InvalidateVisual();
    }

    /// <summary>
    /// Clear background image
    /// </summary>
    public void ClearBackgroundImage()
    {
        _backgroundImage = null;
        InvalidateVisual();
    }

    /// <summary>
    /// Get background image untuk export
    /// </summary>
    public Bitmap? GetBackgroundImage()
    {
        return _backgroundImage;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        var point = e.GetPosition(this);

        if (CurrentTool == DrawingTool.Text)
        {
            AddTextAction(point);
            return;
        }

        _isDrawing = true;

        _currentAction = new DrawingAction
        {
            Tool = CurrentTool,
            Color = CurrentColor,
            StrokeThickness = StrokeThickness,
            BrushStyle = BrushStyle
        };
        _currentAction.Points.Add(point);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!_isDrawing || _currentAction == null) return;

        var point = e.GetPosition(this);

        switch (CurrentTool)
        {
            case DrawingTool.Pencil:
            case DrawingTool.Brush:
            case DrawingTool.Eraser:
                _currentAction.Points.Add(point);
                break;
            case DrawingTool.Line:
            case DrawingTool.Rectangle:
            case DrawingTool.Circle:
            case DrawingTool.Arrow:
                if (_currentAction.Points.Count > 1)
                    _currentAction.Points[1] = point;
                else
                    _currentAction.Points.Add(point);
                break;
        }

        InvalidateVisual();
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_isDrawing && _currentAction != null && _currentAction.Points.Count > 0)
        {
            _actions.Add(_currentAction);
        }

        _isDrawing = false;
        _currentAction = null;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Background putih
        context.FillRectangle(Brushes.White, new Rect(0, 0, Bounds.Width, Bounds.Height));

        // Render background image jika ada
        if (_backgroundImage != null)
        {
            var rect = new Rect(0, 0, Bounds.Width, Bounds.Height);
            context.DrawImage(_backgroundImage, rect);
        }

        // Grid
        if (ShowGrid)
        {
            DrawGrid(context);
        }

        // Gambar semua aksi
        foreach (var action in _actions)
        {
            DrawAction(context, action);
        }

        // Aksi sedang berlangsung
        if (_isDrawing && _currentAction != null)
        {
            DrawAction(context, _currentAction);
        }
    }

    private void DrawAction(DrawingContext context, DrawingAction action)
    {
        if (action.Points.Count == 0) return;

        var brush = new SolidColorBrush(action.Color);
        var pen = new Pen(brush, action.StrokeThickness);

        switch (action.Tool)
        {
            case DrawingTool.Pencil:
                DrawFreehand(context, action, pen);
                break;
            case DrawingTool.Brush:
                DrawBrush(context, action);
                break;
            case DrawingTool.Eraser:
                DrawEraser(context, action);
                break;
            case DrawingTool.Line:
                if (action.Points.Count >= 2)
                    context.DrawLine(pen, action.Points[0], action.Points[1]);
                break;
            case DrawingTool.Rectangle:
                if (action.Points.Count >= 2)
                {
                    var rect = new Rect(action.Points[0], action.Points[1]);
                    context.DrawRectangle(null, pen, rect);
                }
                break;
            case DrawingTool.Circle:
                if (action.Points.Count >= 2)
                {
                    var p1 = action.Points[0];
                    var p2 = action.Points[1];
                    var radiusX = Math.Abs(p2.X - p1.X) / 2;
                    var radiusY = Math.Abs(p2.Y - p1.Y) / 2;
                    var center = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                    context.DrawEllipse(null, pen, center, radiusX, radiusY);
                }
                break;
            case DrawingTool.Arrow:
                DrawArrow(context, action, pen);
                break;
            case DrawingTool.Text:
                DrawText(context, action);
                break;
        }
    }

    private void DrawFreehand(DrawingContext context, DrawingAction action, Pen pen)
    {
        for (int i = 0; i < action.Points.Count - 1; i++)
        {
            context.DrawLine(pen, action.Points[i], action.Points[i + 1]);
        }
    }

    private void DrawBrush(DrawingContext context, DrawingAction action)
    {
        var thickness = action.StrokeThickness;
        var color = action.Color;
        
        switch (action.BrushStyle)
        {
            case BrushStyle.Oil:
                thickness *= 1.5;
                break;
            case BrushStyle.Watercolor:
                color = Color.FromArgb(128, action.Color.R, action.Color.G, action.Color.B);
                thickness *= 2;
                break;
            case BrushStyle.Calligraphy:
                thickness *= 2;
                break;
        }

        var pen = new Pen(new SolidColorBrush(color), thickness);
        
        for (int i = 0; i < action.Points.Count - 1; i++)
        {
            context.DrawLine(pen, action.Points[i], action.Points[i + 1]);
        }
    }

    private void DrawEraser(DrawingContext context, DrawingAction action)
    {
        var eraserPen = new Pen(Brushes.White, action.StrokeThickness * 3);
        for (int i = 0; i < action.Points.Count - 1; i++)
        {
            context.DrawLine(eraserPen, action.Points[i], action.Points[i + 1]);
        }
    }

    private void DrawArrow(DrawingContext context, DrawingAction action, Pen pen)
    {
        if (action.Points.Count >= 2)
        {
            var p1 = action.Points[0];
            var p2 = action.Points[1];
            
            context.DrawLine(pen, p1, p2);
            
            var angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            var arrowLength = 15;
            var arrowAngle = Math.PI / 6;
            
            var arrow1 = new Point(
                p2.X - arrowLength * Math.Cos(angle - arrowAngle),
                p2.Y - arrowLength * Math.Sin(angle - arrowAngle));
            var arrow2 = new Point(
                p2.X - arrowLength * Math.Cos(angle + arrowAngle),
                p2.Y - arrowLength * Math.Sin(angle + arrowAngle));
            
            context.DrawLine(pen, p2, arrow1);
            context.DrawLine(pen, p2, arrow2);
        }
    }

    private void DrawText(DrawingContext context, DrawingAction action)
    {
        if (string.IsNullOrWhiteSpace(action.Text)) return;

        var point = action.Points[0];
        var typeface = new Typeface(new FontFamily(action.FontFamilyName ?? "Arial"));
        var formatted = new FormattedText(
            action.Text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            action.FontSize > 0 ? action.FontSize : 24,
            new SolidColorBrush(action.Color));

        context.DrawText(formatted, point);
    }

    private void DrawGrid(DrawingContext context)
    {
        var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(50, 128, 128, 128)), 1);
        var gridSize = 20;

        for (double x = 0; x < Bounds.Width; x += gridSize)
        {
            context.DrawLine(gridPen, new Point(x, 0), new Point(x, Bounds.Height));
        }

        for (double y = 0; y < Bounds.Height; y += gridSize)
        {
            context.DrawLine(gridPen, new Point(0, y), new Point(Bounds.Width, y));
        }
    }

    private void AddTextAction(Point point)
    {
        var text = CurrentText ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text)) return;

        var action = new DrawingAction
        {
            Tool = DrawingTool.Text,
            Color = CurrentColor,
            StrokeThickness = StrokeThickness,
            BrushStyle = BrushStyle,
            Text = text,
            FontSize = CurrentFontSize,
            FontFamilyName = CurrentFontFamily
        };

        action.Points.Add(point);
        _actions.Add(action);
        InvalidateVisual();
    }

    public void Undo()
    {
        if (_actions.Count > 0)
        {
            _actions.RemoveAt(_actions.Count - 1);
            InvalidateVisual();
        }
    }

    public void Clear()
    {
        _actions.Clear();
        _backgroundImage = null;
        InvalidateVisual();
    }
}

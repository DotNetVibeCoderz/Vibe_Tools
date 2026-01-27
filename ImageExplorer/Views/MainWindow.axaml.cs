using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using ImageExplorer.ViewModels;
using ImageExplorer.Models;
using System.Linq;
using Avalonia;
using Avalonia.Threading;
using System;
using System.Collections.Generic;

namespace ImageExplorer.Views
{
    public partial class MainWindow : Window
    {
        private Point _startPoint;
        private Control? _currentShape;
        private bool _isDrawing;

        public MainWindow()
        {
            InitializeComponent();
            
            // Hook up mouse wheel for zooming
            if (ImageScrollViewer != null)
            {
                ImageScrollViewer.PointerWheelChanged += ImageScrollViewer_PointerWheelChanged;
            }
        }
        
        protected override void OnOpened(System.EventArgs e)
        {
            base.OnOpened(e);
            if (DataContext is MainViewModel vm)
            {
                vm.SetDialogService(new DialogService(this));
                vm.PropertyChanged += ViewModel_PropertyChanged;
                vm.ClearAnnotationsRequested += OnClearAnnotationsRequested;
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
             if (DataContext is MainViewModel vm)
            {
                vm.PropertyChanged -= ViewModel_PropertyChanged;
                vm.ClearAnnotationsRequested -= OnClearAnnotationsRequested;
            }
            base.OnClosing(e);
        }

        private void OnClearAnnotationsRequested(object? sender, EventArgs e)
        {
             AnnotationCanvas.Children.Clear();
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Auto fit when a new image is loaded
            if (e.PropertyName == nameof(MainViewModel.CurrentImage))
            {
                Dispatcher.UIThread.Post(AutoFitImage, DispatcherPriority.Loaded);
            }
        }

        private void FitToScreen_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            AutoFitImage();
        }

        private void AutoFitImage()
        {
            if (DataContext is not MainViewModel vm || vm.CurrentImage == null) return;
            
            var viewportWidth = ImageScrollViewer.Bounds.Width;
            var viewportHeight = ImageScrollViewer.Bounds.Height;
            
            if (viewportWidth <= 0 || viewportHeight <= 0) return;

            var imgW = vm.CurrentImage.Size.Width;
            var imgH = vm.CurrentImage.Size.Height;

            if (imgW <= 0 || imgH <= 0) return;

            double padding = 20;
            double availableW = viewportWidth - padding;
            double availableH = viewportHeight - padding;

            if (availableW <= 0) availableW = viewportWidth;
            if (availableH <= 0) availableH = viewportHeight;

            double scaleX = availableW / imgW;
            double scaleY = availableH / imgH;
            
            double finalScale = Math.Min(scaleX, scaleY);
            
            vm.ZoomScale = finalScale;
        }

        private void ImageScrollViewer_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
        {
            if (DataContext is not MainViewModel vm) return;

            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                if (e.Delta.Y > 0) vm.ZoomIn();
                else vm.ZoomOut();
                e.Handled = true; 
            }
        }

        private void TreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && e.AddedItems.Count > 0 && e.AddedItems[0] is DirectoryNode node)
            {
                vm.SelectedDirectory = node;
            }
        }

        private void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is not MainViewModel vm) return;
            
            var tool = vm.SelectedTool;
            if (tool == "None" || tool == "Pointer") return;

            _isDrawing = true;
            _startPoint = e.GetPosition(AnnotationCanvas);

            var brush = new SolidColorBrush(vm.BrushColor);
            var thickness = vm.BrushThickness;
            
            if (tool == "Pen")
            {
                // Freehand drawing logic using Polyline
                var polyline = new Polyline
                {
                    Stroke = brush,
                    StrokeThickness = thickness,
                    Points = new Points { _startPoint }
                };
                AnnotationCanvas.Children.Add(polyline);
                _currentShape = polyline;
            }
            else if (tool == "Line")
            {
                _currentShape = new Line
                {
                    StartPoint = _startPoint,
                    EndPoint = _startPoint,
                    Stroke = brush,
                    StrokeThickness = thickness
                };
                AnnotationCanvas.Children.Add(_currentShape);
            }
            else if (tool == "Rect")
            {
                _currentShape = new Rectangle
                {
                    Width = 0,
                    Height = 0,
                    Stroke = brush,
                    StrokeThickness = thickness
                };
                Canvas.SetLeft(_currentShape, _startPoint.X);
                Canvas.SetTop(_currentShape, _startPoint.Y);
                AnnotationCanvas.Children.Add(_currentShape);
            }
            else if (tool == "Ellipse")
            {
                _currentShape = new Ellipse
                {
                    Width = 0,
                    Height = 0,
                    Stroke = brush,
                    StrokeThickness = thickness
                };
                Canvas.SetLeft(_currentShape, _startPoint.X);
                Canvas.SetTop(_currentShape, _startPoint.Y);
                AnnotationCanvas.Children.Add(_currentShape);
            }
        }

        private void Canvas_PointerMoved(object? sender, PointerEventArgs e)
        {
            if (!_isDrawing || _currentShape == null) return;

            var currentPoint = e.GetPosition(AnnotationCanvas);

            if (_currentShape is Polyline poly)
            {
                // Add points to polyline for continuous drawing
                poly.Points.Add(currentPoint);
            }
            else if (_currentShape is Line line)
            {
                line.EndPoint = currentPoint;
            }
            else
            {
                var x = Math.Min(_startPoint.X, currentPoint.X);
                var y = Math.Min(_startPoint.Y, currentPoint.Y);
                var w = Math.Abs(_startPoint.X - currentPoint.X);
                var h = Math.Abs(_startPoint.Y - currentPoint.Y);

                if (_currentShape is Rectangle rect)
                {
                    Canvas.SetLeft(rect, x);
                    Canvas.SetTop(rect, y);
                    rect.Width = w;
                    rect.Height = h;
                }
                else if (_currentShape is Ellipse el)
                {
                    Canvas.SetLeft(el, x);
                    Canvas.SetTop(el, y);
                    el.Width = w;
                    el.Height = h;
                }
            }
        }

        private void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!_isDrawing) return;
            
            if (DataContext is MainViewModel vm && _currentShape != null)
            {
                // Finalize shape and add to ViewModel
                
                if (_currentShape is Polyline poly)
                {
                     // Convert Avalonia.Points to List<Avalonia.Point> for Model
                     var pointsList = new List<Point>(poly.Points);
                     AddPenAnnotationToViewModel(vm, pointsList);
                }
                else if (_currentShape is Line line)
                {
                    AddAnnotationToViewModel(vm, AnnotationType.Line, line.StartPoint.X, line.StartPoint.Y, line.EndPoint.X, line.EndPoint.Y);
                }
                else if (_currentShape is Rectangle rect)
                {
                    var left = Canvas.GetLeft(rect);
                    var top = Canvas.GetTop(rect);
                    AddAnnotationToViewModel(vm, AnnotationType.Rectangle, left, top, rect.Width, rect.Height);
                }
                else if (_currentShape is Ellipse ellipse)
                {
                     var left = Canvas.GetLeft(ellipse);
                    var top = Canvas.GetTop(ellipse);
                    AddAnnotationToViewModel(vm, AnnotationType.Ellipse, left, top, ellipse.Width, ellipse.Height);
                }
            }

            _isDrawing = false;
            _currentShape = null;
        }

        private void AddPenAnnotationToViewModel(MainViewModel vm, List<Point> points)
        {
             if (vm.CurrentImage == null) return;

            var item = new AnnotationItem
            {
                Type = AnnotationType.Pen,
                Color = vm.BrushColor,
                Thickness = vm.BrushThickness,
                Points = points
            };
            
            vm.Annotations.Add(item);
        }

        private void AddAnnotationToViewModel(MainViewModel vm, AnnotationType type, double x, double y, double wOrX2, double hOrY2)
        {
            if (vm.CurrentImage == null) return;
            
            var item = new AnnotationItem
            {
                Type = type,
                Color = vm.BrushColor,
                Thickness = vm.BrushThickness,
                X = x,
                Y = y,
            };

            if (type == AnnotationType.Line)
            {
                item.EndX = wOrX2;
                item.EndY = hOrY2;
            }
            else
            {
                item.Width = wOrX2;
                item.Height = hOrY2;
            }
            
            vm.Annotations.Add(item);
        }
    }
}
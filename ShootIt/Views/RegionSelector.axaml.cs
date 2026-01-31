using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using System;

namespace ShootIt.Views
{
    public partial class RegionSelector : Window
    {
        private Point _startPoint;
        private bool _isSelecting;
        private Rectangle _selectionRect;

        public event Action<Rect> RegionSelected;

        public RegionSelector()
        {
            InitializeComponent();
            _selectionRect = this.FindControl<Rectangle>("SelectionRect");
            
            // Wire up events manually to ensure they fire
            this.PointerPressed += OnPointerPressed;
            this.PointerMoved += OnPointerMoved;
            this.PointerReleased += OnPointerReleased;
            this.KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }

        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            _isSelecting = true;
            _startPoint = e.GetPosition(this);
            _selectionRect.IsVisible = true;
            Canvas.SetLeft(_selectionRect, _startPoint.X);
            Canvas.SetTop(_selectionRect, _startPoint.Y);
            _selectionRect.Width = 0;
            _selectionRect.Height = 0;
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (!_isSelecting) return;

            var currentPoint = e.GetPosition(this);
            
            var x = Math.Min(_startPoint.X, currentPoint.X);
            var y = Math.Min(_startPoint.Y, currentPoint.Y);
            var width = Math.Abs(_startPoint.X - currentPoint.X);
            var height = Math.Abs(_startPoint.Y - currentPoint.Y);

            Canvas.SetLeft(_selectionRect, x);
            Canvas.SetTop(_selectionRect, y);
            _selectionRect.Width = width;
            _selectionRect.Height = height;
        }

        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (!_isSelecting) return;
            _isSelecting = false;

            var currentPoint = e.GetPosition(this);
            var x = Math.Min(_startPoint.X, currentPoint.X);
            var y = Math.Min(_startPoint.Y, currentPoint.Y);
            var width = Math.Abs(_startPoint.X - currentPoint.X);
            var height = Math.Abs(_startPoint.Y - currentPoint.Y);

            // Hide window before firing event so the window itself doesn't appear in the screenshot
            this.Hide();
            
            // Adjust coordinates to screen coordinates if needed, 
            // but for full screen overlay, window coords ~= screen coords (mostly)
            // On High DPI setups, scaling might be needed.
            
            // Calculate screen pixel scaling
            var screen = this.Screens.Primary;
            double scaling = screen.Scaling; 

            // IMPORTANT: System.Drawing expects physical pixels.
            // Avalonia uses logical pixels. We must convert.
            
            RegionSelected?.Invoke(new Rect(x * scaling, y * scaling, width * scaling, height * scaling));
            this.Close();
        }
    }
}
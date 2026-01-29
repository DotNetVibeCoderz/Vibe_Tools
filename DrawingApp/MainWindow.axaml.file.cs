using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Media.Imaging;
using DrawingApp.Controls;
using DrawingApp.Models;
using DrawingApp.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DrawingApp;

public partial class MainWindow : Window
{
    private DrawingCanvas? _canvas;
    private DrawingTool _currentTool = DrawingTool.Pencil;
    private Color _currentColor = Colors.Black;
    private double _strokeThickness = 2;
    private BrushStyle _brushStyle = BrushStyle.Normal;
    private bool _showGrid = false;

    // Text settings
    private string _currentText = "";
    private string _currentFontFamily = "Arial";
    private double _currentFontSize = 24;

    public MainWindow()
    {
        InitializeComponent();
        SetupEventHandlers();
        SetupTextControls();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _canvas = this.FindControl<DrawingCanvas>("DrawingCanvas");
    }

    private void SetupEventHandlers()
    {
        // Menu
        var menuNew = this.FindControl<MenuItem>("MenuNew");
        var menuOpen = this.FindControl<MenuItem>("MenuOpen");
        var menuSave = this.FindControl<MenuItem>("MenuSave");
        var menuExit = this.FindControl<MenuItem>("MenuExit");
        var menuUndo = this.FindControl<MenuItem>("MenuUndo");
        var menuClear = this.FindControl<MenuItem>("MenuClear");
        var menuGrid = this.FindControl<MenuItem>("MenuGrid");
        var menuAbout = this.FindControl<MenuItem>("MenuAbout");

        if (menuNew != null) menuNew.Click += (s, e) => NewFile();
        if (menuOpen != null) menuOpen.Click += async (s, e) => await OpenFileAsync();
        if (menuSave != null) menuSave.Click += async (s, e) => await SaveFileAsync();
        if (menuExit != null) menuExit.Click += (s, e) => Close();
        if (menuUndo != null) menuUndo.Click += (s, e) => Undo();
        if (menuClear != null) menuClear.Click += (s, e) => Clear();
        if (menuGrid != null) menuGrid.Click += (s, e) => ToggleGrid();
        if (menuAbout != null) menuAbout.Click += (s, e) => ShowAbout();

        // Toolbar
        var btnNew = this.FindControl<Button>("BtnNew");
        var btnOpen = this.FindControl<Button>("BtnOpen");
        var btnSave = this.FindControl<Button>("BtnSave");
        var btnUndo = this.FindControl<Button>("BtnUndo");
        var btnClear = this.FindControl<Button>("BtnClear");
        
        if (btnNew != null) btnNew.Click += (s, e) => NewFile();
        if (btnOpen != null) btnOpen.Click += async (s, e) => await OpenFileAsync();
        if (btnSave != null) btnSave.Click += async (s, e) => await SaveFileAsync();
        if (btnUndo != null) btnUndo.Click += (s, e) => Undo();
        if (btnClear != null) btnClear.Click += (s, e) => Clear();

        // Tool buttons
        var btnPencil = this.FindControl<Button>("BtnPencil");
        var btnBrush = this.FindControl<Button>("BtnBrush");
        var btnLine = this.FindControl<Button>("BtnLine");
        var btnRectangle = this.FindControl<Button>("BtnRectangle");
        var btnCircle = this.FindControl<Button>("BtnCircle");
        var btnArrow = this.FindControl<Button>("BtnArrow");
        var btnEraser = this.FindControl<Button>("BtnEraser");
        var btnText = this.FindControl<Button>("BtnText");

        if (btnPencil != null) btnPencil.Click += (s, e) => SetTool(DrawingTool.Pencil);
        if (btnBrush != null) btnBrush.Click += (s, e) => SetTool(DrawingTool.Brush);
        if (btnLine != null) btnLine.Click += (s, e) => SetTool(DrawingTool.Line);
        if (btnRectangle != null) btnRectangle.Click += (s, e) => SetTool(DrawingTool.Rectangle);
        if (btnCircle != null) btnCircle.Click += (s, e) => SetTool(DrawingTool.Circle);
        if (btnArrow != null) btnArrow.Click += (s, e) => SetTool(DrawingTool.Arrow);
        if (btnEraser != null) btnEraser.Click += (s, e) => SetTool(DrawingTool.Eraser);
        if (btnText != null) btnText.Click += (s, e) => SetTool(DrawingTool.Text);

        // Stroke slider
        var strokeSlider = this.FindControl<Slider>("StrokeSlider");
        if (strokeSlider != null)
        {
            strokeSlider.PropertyChanged += (s, e) =>
            {
                if (e.Property.Name == "Value")
                {
                    _strokeThickness = strokeSlider.Value;
                    if (_canvas != null)
                        _canvas.StrokeThickness = _strokeThickness;
                    
                    var strokeLabel = this.FindControl<TextBlock>("StrokeLabel");
                    if (strokeLabel != null)
                        strokeLabel.Text = $"{_strokeThickness:0} px";
                }
            };
        }

        // Color buttons
        SetupColorButton("ColorBlack", Colors.Black);
        SetupColorButton("ColorWhite", Colors.White);
        SetupColorButton("ColorRed", Colors.Red);
        SetupColorButton("ColorGreen", Colors.Green);
        SetupColorButton("ColorBlue", Colors.Blue);
        SetupColorButton("ColorYellow", Colors.Yellow);
        SetupColorButton("ColorOrange", Colors.Orange);
        SetupColorButton("ColorPurple", Colors.Purple);
        SetupColorButton("ColorPink", Colors.Pink);
        SetupColorButton("ColorBrown", Colors.Brown);
        SetupColorButton("ColorGray", Colors.Gray);
        SetupColorButton("ColorCyan", Colors.Cyan);

        // Brush style radio buttons
        var brushNormal = this.FindControl<RadioButton>("BrushNormal");
        var brushOil = this.FindControl<RadioButton>("BrushOil");
        var brushWatercolor = this.FindControl<RadioButton>("BrushWatercolor");
        var brushCalligraphy = this.FindControl<RadioButton>("BrushCalligraphy");

        if (brushNormal != null) brushNormal.Click += (s, e) => SetBrushStyle(BrushStyle.Normal);
        if (brushOil != null) brushOil.Click += (s, e) => SetBrushStyle(BrushStyle.Oil);
        if (brushWatercolor != null) brushWatercolor.Click += (s, e) => SetBrushStyle(BrushStyle.Watercolor);
        if (brushCalligraphy != null) brushCalligraphy.Click += (s, e) => SetBrushStyle(BrushStyle.Calligraphy);
    }

    private void SetupTextControls()
    {
        var textInput = this.FindControl<TextBox>("TxtTextInput");
        var fontFamilyCombo = this.FindControl<ComboBox>("FontFamilyCombo");
        var fontSizeCombo = this.FindControl<ComboBox>("FontSizeCombo");

        if (textInput != null)
        {
            textInput.Text = "";
            textInput.PropertyChanged += (s, e) =>
            {
                if (e.Property.Name == "Text")
                {
                    _currentText = textInput.Text ?? "";
                    if (_canvas != null)
                        _canvas.CurrentText = _currentText;
                }
            };
        }

        if (fontFamilyCombo != null)
        {
            var fontFamilies = FontManager.Current.SystemFonts
                .Select(f => f.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            fontFamilyCombo.ItemsSource = fontFamilies;
            fontFamilyCombo.SelectedItem = _currentFontFamily;
            fontFamilyCombo.SelectionChanged += (s, e) =>
            {
                if (fontFamilyCombo.SelectedItem is string name)
                {
                    _currentFontFamily = name;
                    if (_canvas != null)
                        _canvas.CurrentFontFamily = name;
                }
            };
        }

        if (fontSizeCombo != null)
        {
            var sizes = new List<int> { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 28, 32, 36, 48, 64, 72, 96 };
            fontSizeCombo.ItemsSource = sizes;
            fontSizeCombo.SelectedItem = (int)_currentFontSize;
            fontSizeCombo.SelectionChanged += (s, e) =>
            {
                if (fontSizeCombo.SelectedItem is int size)
                {
                    _currentFontSize = size;
                    if (_canvas != null)
                        _canvas.CurrentFontSize = size;
                }
            };
        }

        // Set default values to canvas
        if (_canvas != null)
        {
            _canvas.CurrentText = _currentText;
            _canvas.CurrentFontFamily = _currentFontFamily;
            _canvas.CurrentFontSize = _currentFontSize;
        }
    }

    private void SetupColorButton(string name, Color color)
    {
        var btn = this.FindControl<Button>(name);
        if (btn != null)
        {
            btn.Click += (s, e) => SetColor(color);
        }
    }

    private void SetTool(DrawingTool tool)
    {
        _currentTool = tool;
        if (_canvas != null)
            _canvas.CurrentTool = tool;
        
        var toolLabel = this.FindControl<TextBlock>("ToolLabel");
        if (toolLabel != null)
            toolLabel.Text = $"Tool: {tool}";
        
        UpdateStatus($"✓ Selected: {tool}");
    }

    private void SetColor(Color color)
    {
        _currentColor = color;
        if (_canvas != null)
            _canvas.CurrentColor = color;
        
        var colorDisplay = this.FindControl<Border>("CurrentColorDisplay");
        if (colorDisplay != null)
            colorDisplay.Background = new SolidColorBrush(color);
        
        UpdateStatus($"🎨 Color changed to: {color}");
    }

    private void SetBrushStyle(BrushStyle style)
    {
        _brushStyle = style;
        if (_canvas != null)
            _canvas.BrushStyle = style;
        
        UpdateStatus($"🖌️ Brush Style: {style}");
    }

    private void NewFile()
    {
        if (_canvas != null)
            _canvas.Clear();
        UpdateStatus("📄 New file created - Ready to draw!");
    }

    /// <summary>
    /// Open file gambar ke canvas (PNG/JPG/BMP/GIF)
    /// </summary>
    private async Task OpenFileAsync()
    {
        if (_canvas == null)
        {
            UpdateStatus("❌ No canvas to load!");
            return;
        }

        try
        {
            var storage = StorageProvider;

            var fileTypes = new List<FilePickerFileType>
            {
                new("Image Files") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" } },
                new("PNG Image") { Patterns = new[] { "*.png" } },
                new("JPEG Image") { Patterns = new[] { "*.jpg", "*.jpeg" } },
                new("Bitmap Image") { Patterns = new[] { "*.bmp" } },
                new("GIF Image") { Patterns = new[] { "*.gif" } },
                new("All Files") { Patterns = new[] { "*.*" } }
            };

            var options = new FilePickerOpenOptions
            {
                Title = "Open Image...",
                AllowMultiple = false,
                FileTypeFilter = fileTypes
            };

            var result = await storage.OpenFilePickerAsync(options);

            if (result != null && result.Count > 0)
            {
                var file = result[0];
                UpdateStatus("📂 Loading image...");

                await using var stream = await file.OpenReadAsync();
                var bitmap = new Bitmap(stream);

                // Reset canvas dan load gambar sebagai background
                _canvas.Clear();
                _canvas.LoadImage(bitmap);

                var fileName = Path.GetFileName(file.Path.LocalPath);
                UpdateStatus($"✅ Image loaded: {fileName}");
            }
            else
            {
                UpdateStatus("⚠️ Open cancelled");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"❌ Error opening file: {ex.Message}");
        }
    }

    /// <summary>
    /// Save file dengan dialog storage provider (modern API)
    /// </summary>
    private async Task SaveFileAsync()
    {
        if (_canvas == null)
        {
            UpdateStatus("❌ No canvas to save!");
            return;
        }

        try
        {
            var storage = StorageProvider;
            
            // File type choices
            var fileTypes = new List<FilePickerFileType>
            {
                new("PNG Image") { Patterns = new[] { "*.png" } },
                new("JPEG Image") { Patterns = new[] { "*.jpg", "*.jpeg" } },
                new("Bitmap Image") { Patterns = new[] { "*.bmp" } },
                new("GIF Image") { Patterns = new[] { "*.gif" } },
                new("All Files") { Patterns = new[] { "*.*" } }
            };

            var options = new FilePickerSaveOptions
            {
                Title = "Save Drawing As...",
                FileTypeChoices = fileTypes,
                DefaultExtension = "png",
                SuggestedFileName = "drawing.png"
            };

            var result = await storage.SaveFilePickerAsync(options);

            if (result != null)
            {
                UpdateStatus("💾 Saving image...");
                
                var path = result.Path.LocalPath;
                bool success = ImageExporter.ExportToFile(_canvas, path);
                
                if (success)
                {
                    var fileName = Path.GetFileName(path);
                    var extension = Path.GetExtension(path).ToUpper();
                    UpdateStatus($"✅ Successfully saved as: {fileName} ({extension})");
                }
                else
                {
                    UpdateStatus("❌ Failed to save image!");
                }
            }
            else
            {
                UpdateStatus("⚠️ Save cancelled");
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"❌ Error saving file: {ex.Message}");
        }
    }

    private void Undo()
    {
        if (_canvas != null)
            _canvas.Undo();
        UpdateStatus("↶ Undo - Last action removed");
    }

    private void Clear()
    {
        if (_canvas != null)
            _canvas.Clear();
        UpdateStatus("🗑️ Canvas cleared - Start fresh!");
    }

    private void ToggleGrid()
    {
        _showGrid = !_showGrid;
        if (_canvas != null)
        {
            _canvas.ShowGrid = _showGrid;
            _canvas.InvalidateVisual();
        }
        UpdateStatus($"📐 Grid: {(_showGrid ? "ON ✓" : "OFF")}");
    }

    private void ShowAbout()
    {
        var aboutDialog = new Window
        {
            Title = "About Drawing App",
            Width = 450,
            Height = 350,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Background = new SolidColorBrush(Color.FromRgb(245, 245, 245))
        };

        var content = new StackPanel
        {
            Margin = new Avalonia.Thickness(30),
            Spacing = 15
        };

        content.Children.Add(new TextBlock
        {
            Text = "🎨 Drawing App",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });

        content.Children.Add(new TextBlock
        {
            Text = "MS Paint Clone - Enhanced Edition",
            FontSize = 16,
            Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        });

        content.Children.Add(new Separator { Height = 1, Background = new SolidColorBrush(Color.FromRgb(221, 221, 221)) });

        content.Children.Add(new TextBlock
        {
            Text = "Version 2.0",
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51))
        });

        content.Children.Add(new TextBlock
        {
            Text = "✨ New Features:\n• Save to PNG, JPEG, BMP, GIF\n• Improved UI with better contrast\n• Enhanced user experience\n• Cross-platform support",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
            LineHeight = 22
        });

        content.Children.Add(new Separator { Height = 1, Background = new SolidColorBrush(Color.FromRgb(221, 221, 221)) });

        content.Children.Add(new TextBlock
        {
            Text = "Created by: Jacky the Code Bender",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51))
        });

        content.Children.Add(new TextBlock
        {
            Text = "Built with ❤️ using Avalonia UI",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102))
        });

        var okButton = new Button
        {
            Content = "OK",
            Width = 100,
            Height = 35,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 10, 0, 0)
        };

        okButton.Click += (s, e) => aboutDialog.Close();
        content.Children.Add(okButton);

        aboutDialog.Content = content;
        aboutDialog.ShowDialog(this);
        
        UpdateStatus("ℹ️ About dialog opened");
    }

    private void UpdateStatus(string message)
    {
        var statusLabel = this.FindControl<TextBlock>("StatusLabel");
        if (statusLabel != null)
            statusLabel.Text = message;
    }
}

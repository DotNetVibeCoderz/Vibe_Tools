using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.IO;
using System.Threading.Tasks;
using Img2Ascii;

namespace Img2Ascii
{
    public partial class MainWindow : Window
    {
        private string? _currentFilePath;
        
        // Ramps matching AsciiEngine
        private readonly string[] _ramps = new[]
        {
            AsciiEngine.RampStandard,
            AsciiEngine.RampComplex,
            AsciiEngine.RampMinimal
        };

        public MainWindow()
        {
            InitializeComponent();
            
            // Wire events
            var browseBtn = this.FindControl<Button>("BrowseButton");
            if (browseBtn != null) browseBtn.Click += BrowseButton_Click;

            var exportBtn = this.FindControl<Button>("ExportButton");
            if (exportBtn != null) exportBtn.Click += ExportButton_Click;

            var slider = this.FindControl<Slider>("ResolutionSlider");
            if (slider != null)
            {
                slider.PropertyChanged += (s, e) =>
                {
                    if (e.Property.Name == "Value")
                    {
                         UpdatePreview();
                    }
                };
            }

            var combo = this.FindControl<ComboBox>("CharacterSetCombo");
            if (combo != null) combo.SelectionChanged += (s, e) => UpdatePreview();

            var check = this.FindControl<CheckBox>("InvertCheck");
            if (check != null)
            {
                check.IsCheckedChanged += (s, e) => UpdatePreview();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Image",
                AllowMultiple = false,
                FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
            });

            if (files.Count >= 1)
            {
                _currentFilePath = files[0].Path.LocalPath;
                var statusText = this.FindControl<TextBlock>("StatusText");
                if (statusText != null) statusText.Text = $"Loaded: {Path.GetFileName(_currentFilePath)}";
                
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            if (string.IsNullOrEmpty(_currentFilePath)) return;

            var slider = this.FindControl<Slider>("ResolutionSlider");
            var combo = this.FindControl<ComboBox>("CharacterSetCombo");
            var check = this.FindControl<CheckBox>("InvertCheck");
            var output = this.FindControl<TextBlock>("OutputText");
            var statusText = this.FindControl<TextBlock>("StatusText");

            if (slider == null || combo == null || check == null || output == null) return;

            int width = (int)slider.Value;
            
            int rampIndex = combo.SelectedIndex;
            if (rampIndex < 0 || rampIndex >= _ramps.Length) rampIndex = 0;
            
            string ramp = _ramps[rampIndex];
            bool invert = check.IsChecked ?? false;

            if (statusText != null) statusText.Text = "Converting...";

            Task.Run(() => 
            {
                try 
                {
                    string ascii = AsciiEngine.Convert(_currentFilePath, width, ramp, invert);
                    
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        output.Text = ascii;
                        if (statusText != null) statusText.Text = "Conversion Complete.";
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (statusText != null) statusText.Text = $"Error: {ex.Message}";
                    });
                }
            });
        }

        private async void ExportButton_Click(object? sender, RoutedEventArgs e)
        {
            var output = this.FindControl<TextBlock>("OutputText");
            if (output == null || string.IsNullOrEmpty(output.Text)) return;

            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save ASCII Art",
                DefaultExtension = "txt",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Text Files") { Patterns = new[] { "*.txt" } }
                }
            });

            if (file != null)
            {
                try
                {
                    await using var stream = await file.OpenWriteAsync();
                    using var writer = new StreamWriter(stream);
                    await writer.WriteAsync(output.Text);
                    
                    var statusText = this.FindControl<TextBlock>("StatusText");
                    if (statusText != null) statusText.Text = "Saved successfully!";
                }
                catch (Exception ex)
                {
                    var statusText = this.FindControl<TextBlock>("StatusText");
                    if (statusText != null) statusText.Text = $"Save failed: {ex.Message}";
                }
            }
        }
    }
}
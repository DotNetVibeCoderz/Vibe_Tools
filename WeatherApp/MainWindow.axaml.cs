using Avalonia; // Penting untuk RelativePoint, RelativeUnit
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WeatherApp
{
    public partial class MainWindow : Window
    {
        private WeatherService _weatherService;
        private List<City> _cities;

        // Kontrol UI (akan di-resolve saat runtime)
        private ComboBox? _cityCombo;
        private TextBlock? _tempText;
        private TextBlock? _descText;
        private TextBlock? _timeText;
        private TextBlock? _windText;
        private TextBlock? _coordText;
        private Border? _mainBackground;
        
        // Icons
        private Control? _sunIcon;
        private Control? _cloudIcon;
        private Control? _rainIcon;

        public MainWindow()
        {
            InitializeComponent();
            _weatherService = new WeatherService();
            InitializeData();
            
            // Resolve controls by name
            _cityCombo = this.FindControl<ComboBox>("CityComboBox");
            _tempText = this.FindControl<TextBlock>("TempText");
            _descText = this.FindControl<TextBlock>("DescText");
            _timeText = this.FindControl<TextBlock>("TimeText");
            _windText = this.FindControl<TextBlock>("WindText");
            _coordText = this.FindControl<TextBlock>("CoordText");
            _mainBackground = this.FindControl<Border>("MainBackground");
            
            _sunIcon = this.FindControl<Control>("SunIcon");
            _cloudIcon = this.FindControl<Control>("CloudIcon");
            _rainIcon = this.FindControl<Control>("RainIcon");

            // Setup combo box
            if (_cityCombo != null)
            {
                _cityCombo.ItemsSource = _cities.Select(c => c.Name).ToList();
                _cityCombo.SelectionChanged += OnCityChanged;
                _cityCombo.SelectedIndex = 0; // Trigger load default
            }
        }

        private void InitializeComponent()
        {
            Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
        }

        private void InitializeData()
        {
            _cities = new List<City>
            {
                new City("Jakarta", -6.2088, 106.8456),
                new City("Surabaya", -7.2575, 112.7521),
                new City("Bandung", -6.9175, 107.6191),
                new City("Tokyo", 35.6895, 139.6917),
                new City("London", 51.5074, -0.1278),
                new City("New York", 40.7128, -74.0060),
                new City("Moscow", 55.7558, 37.6173)
            };
        }

        private async void OnCityChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_cityCombo?.SelectedItem == null) return;

            string? selectedName = _cityCombo.SelectedItem.ToString();
            var city = _cities.FirstOrDefault(c => c.Name == selectedName);

            if (city != null)
            {
                // Set loading state
                if (_descText != null) _descText.Text = "Mengambil data...";
                
                var data = await _weatherService.GetWeatherAsync(city.Latitude, city.Longitude);

                if (data != null && data.current_weather != null)
                {
                    Dispatcher.UIThread.Post(() => UpdateUI(data, city));
                }
                else
                {
                     if (_descText != null) _descText.Text = "Gagal mengambil data.";
                }
            }
        }

        private void UpdateUI(WeatherResponse data, City city)
        {
            var weather = data.current_weather;

            // 1. Text Info
            if (_tempText != null) _tempText.Text = $"{weather.temperature}°C";
            if (_descText != null) _descText.Text = _weatherService.GetWeatherDescription(weather.weathercode);
            if (_windText != null) _windText.Text = $"{weather.windspeed} km/h";
            if (_coordText != null) _coordText.Text = $"{data.latitude:F2}, {data.longitude:F2}";
            if (_timeText != null) _timeText.Text = $"Updated: {DateTime.Now:HH:mm}";

            // 2. Icon & Background Logic
            UpdateWeatherVisuals(weather.weathercode, weather.is_day == 1);
        }

        private void UpdateWeatherVisuals(int code, bool isDay)
        {
            // Reset Icons
            if (_sunIcon != null) _sunIcon.IsVisible = false;
            if (_cloudIcon != null) _cloudIcon.IsVisible = false;
            if (_rainIcon != null) _rainIcon.IsVisible = false;

            // Colors
            var dayGradientStart = Color.Parse("#4facfe");
            var dayGradientEnd = Color.Parse("#00f2fe");
            
            var nightGradientStart = Color.Parse("#0f2027"); 
            var nightGradientEnd = Color.Parse("#203a43");

            var rainGradientStart = Color.Parse("#373B44");
            var rainGradientEnd = Color.Parse("#4286f4");

            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative)
            };

            // Simple Logic Mapping
            if (code <= 1) // Clear
            {
                if (_sunIcon != null) _sunIcon.IsVisible = isDay; 
                brush.GradientStops.Add(new GradientStop(isDay ? dayGradientStart : nightGradientStart, 0));
                brush.GradientStops.Add(new GradientStop(isDay ? dayGradientEnd : nightGradientEnd, 1));
            }
            else if (code <= 3) // Cloudy
            {
                if (_cloudIcon != null) _cloudIcon.IsVisible = true;
                brush.GradientStops.Add(new GradientStop(Color.Parse("#757F9A"), 0));
                brush.GradientStops.Add(new GradientStop(Color.Parse("#D7DDE8"), 1));
            }
            else if (code >= 51) // Rain / Drizzle / Thunder
            {
                if (_cloudIcon != null) _cloudIcon.IsVisible = true;
                if (_rainIcon != null) _rainIcon.IsVisible = true;
                brush.GradientStops.Add(new GradientStop(rainGradientStart, 0));
                brush.GradientStops.Add(new GradientStop(rainGradientEnd, 1));
            }
            else // Default
            {
                if (_cloudIcon != null) _cloudIcon.IsVisible = true;
                 brush.GradientStops.Add(new GradientStop(Color.Parse("#606c88"), 0));
                 brush.GradientStops.Add(new GradientStop(Color.Parse("#3f4c6b"), 1));
            }

            // Apply Background
            if (_mainBackground != null) _mainBackground.Background = brush;
        }
    }
}
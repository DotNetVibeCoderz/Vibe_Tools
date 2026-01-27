using System;
using System.Collections.Generic;

namespace WeatherApp
{
    // Model untuk respons dari OpenMeteo API
    public class WeatherResponse
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
        public CurrentWeather? current_weather { get; set; }
    }

    public class CurrentWeather
    {
        public double temperature { get; set; }
        public double windspeed { get; set; }
        public double winddirection { get; set; }
        public int weathercode { get; set; } 
        public int is_day { get; set; } 
        public string? time { get; set; }
    }

    // Model sederhana untuk Kota
    public class City
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public City(string name, double lat, double lon)
        {
            Name = name;
            Latitude = lat;
            Longitude = lon;
        }
    }
}
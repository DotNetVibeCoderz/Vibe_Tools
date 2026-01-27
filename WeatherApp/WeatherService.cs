using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WeatherApp
{
    public class WeatherService
    {
        private readonly HttpClient _client;

        public WeatherService()
        {
            _client = new HttpClient();
            // User-Agent diperlukan oleh beberapa API agar tidak dianggap bot spam
            _client.DefaultRequestHeaders.Add("User-Agent", "WeatherApp-Demo/1.0");
        }

        public async Task<WeatherResponse> GetWeatherAsync(double lat, double lon)
        {
            try
            {
                // Menggunakan Open-Meteo API (Free, No Key)
                string url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";
                var response = await _client.GetStringAsync(url);
                return JsonConvert.DeserializeObject<WeatherResponse>(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching weather: {ex.Message}");
                return null;
            }
        }

        // Helper untuk menerjemahkan WMO Code ke deskripsi teks
        public string GetWeatherDescription(int code)
        {
            return code switch
            {
                0 => "Cerah (Clear Sky)",
                1 => "Cerah Berawan (Mainly Clear)",
                2 => "Berawan (Partly Cloudy)",
                3 => "Mendung (Overcast)",
                45 or 48 => "Kabut (Fog)",
                51 or 53 or 55 => "Gerimis (Drizzle)",
                61 or 63 or 65 => "Hujan (Rain)",
                71 or 73 or 75 => "Salju (Snow)",
                95 or 96 or 99 => "Badai Petir (Thunderstorm)",
                _ => "Tidak diketahui"
            };
        }
    }
}
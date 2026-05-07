using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleETL.Services
{
    public class SettingsService
    {
        private readonly string _settingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        
        public bool IsDarkMode { get; set; } = true;
        public int ApiPort { get; set; } = 5000;

        public event Action? OnChange;

        public SettingsService()
        {
            Load();
        }

        public void Load()
        {
            if (File.Exists(_settingsFile))
            {
                try 
                {
                    var json = File.ReadAllText(_settingsFile);
                    var state = JsonSerializer.Deserialize<SettingsState>(json);
                    if (state != null) 
                    {
                        IsDarkMode = state.IsDarkMode;
                        ApiPort = state.ApiPort;
                    }
                } 
                catch {}
            }
        }

        public void Save()
        {
            var state = new SettingsState { IsDarkMode = this.IsDarkMode, ApiPort = this.ApiPort };
            var json = JsonSerializer.Serialize(state);
            File.WriteAllText(_settingsFile, json);
            OnChange?.Invoke();
        }

        private class SettingsState
        {
            public bool IsDarkMode { get; set; }
            public int ApiPort { get; set; }
        }
    }
}

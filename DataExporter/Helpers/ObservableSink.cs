using System;
using System.Collections.ObjectModel;
using Serilog.Core;
using Serilog.Events;
using Avalonia.Threading;

namespace DataExporter.Helpers
{
    // Custom Sink to push logs to UI
    public class ObservableSink : ILogEventSink
    {
        public static ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

        public void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage();
            var timestamp = logEvent.Timestamp.ToString("HH:mm:ss");
            var level = logEvent.Level.ToString().Substring(0, 3);
            
            Dispatcher.UIThread.Post(() =>
            {
                Logs.Insert(0, $"[{timestamp}] [{level}] {message}");
                // Keep only last 100 logs
                if (Logs.Count > 100) Logs.RemoveAt(Logs.Count - 1);
            });
        }
    }
}

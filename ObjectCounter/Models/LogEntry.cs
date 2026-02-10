using System;

namespace ObjectCounter.Models
{
    public class LogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string ObjectType { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Source { get; set; } = "Camera";
    }
}

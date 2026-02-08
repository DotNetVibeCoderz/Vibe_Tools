using System;

namespace MidiPlayer.Models
{
    public class MidiTrackItem
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
    }
}
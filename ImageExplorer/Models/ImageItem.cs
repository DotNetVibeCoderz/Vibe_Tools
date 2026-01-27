using System;

namespace ImageExplorer.Models
{
    public class ImageItem
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime DateModified { get; set; }
        public string Dimensions { get; set; } = "Unknown"; // Populated when loaded if needed, or lazy loaded
        
        // Helper to format size
        public string SizeDisplay
        {
            get
            {
                if (Size < 1024) return $"{Size} B";
                if (Size < 1024 * 1024) return $"{Size / 1024.0:F2} KB";
                return $"{Size / (1024.0 * 1024.0):F2} MB";
            }
        }
    }
}
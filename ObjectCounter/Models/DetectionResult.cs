namespace ObjectCounter.Models
{
    public class DetectionResult
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public float Confidence { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        
        // Tracking info
        public int TrackId { get; set; } = -1;
    }
}

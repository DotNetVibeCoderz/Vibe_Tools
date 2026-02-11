using System;
using System.ComponentModel.DataAnnotations;

namespace LPRNet.Models
{
    public class PlateRecord
    {
        [Key]
        public int Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string Country { get; set; } = "ID"; // Default to Indonesia
        public bool IsBlacklisted { get; set; }
        public string Notes { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{PlateNumber} - {Timestamp} (Confidence: {Confidence:P0})";
        }
    }
}
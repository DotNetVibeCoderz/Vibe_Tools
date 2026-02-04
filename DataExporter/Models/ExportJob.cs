using System.Collections.Generic;

namespace DataExporter.Models
{
    public class ExportJob
    {
        public string? JobName { get; set; }
        public string? Query { get; set; }
        public string? OutputFormat { get; set; } // csv, json
        public string? OutputPath { get; set; }
        public bool CompressOutput { get; set; }
        public List<string> MaskFields { get; set; } = new List<string>();
    }
}

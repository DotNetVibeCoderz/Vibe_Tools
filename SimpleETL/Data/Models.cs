using System;
using System.Collections.Generic;

namespace SimpleETL.Models
{
    public class DataFlow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string JsonDesign { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public bool IsScheduled { get; set; }
        public string CronExpression { get; set; } = string.Empty;
    }

    public class JobLog
    {
        public int Id { get; set; }
        public int DataFlowId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = "Running"; // Success, Failed, Running
        public string ErrorMessage { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }

    public class FlowNode
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Config { get; set; } = string.Empty;
    }
}

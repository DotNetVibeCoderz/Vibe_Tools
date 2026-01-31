using System;

namespace FacialAttendance.Models
{
    public class AttendanceLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }
        // Helper property for display
        public string UserName { get; set; }
    }
}
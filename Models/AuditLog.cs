using System;

namespace RPACProductionPlanner.Models
{
    public class AuditLog
    {
        public int LogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string TableAffected { get; set; }
        public string RecordId { get; set; } // Changed to string to support work_ticket_no
        public DateTime Timestamp { get; set; }
        public string IPAddress { get; set; }
        
        // Navigation property or UI helper
        public string UserFullName { get; set; }
    }
}

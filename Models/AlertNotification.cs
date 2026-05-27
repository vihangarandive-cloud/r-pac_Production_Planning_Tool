// /Models/AlertNotification.cs
using System;

namespace RPACProductionPlanner.Models
{
    public class AlertNotification
    {
        public int AlertId { get; set; }
        public string AlertType { get; set; }
        public string Message { get; set; }
        public int? RelatedId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

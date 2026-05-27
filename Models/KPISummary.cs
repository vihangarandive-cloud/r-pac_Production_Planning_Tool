// /Models/KPISummary.cs
namespace RPACProductionPlanner.Models
{
    public class KPISummary
    {
        public int TotalOrders { get; set; }     // Sum of the 4 buckets below (meaningful scope only)
        public int ActiveOrders { get; set; }    // Released to floor / In Progress
        public int CompletedOrders { get; set; } // Completed or Closed
        public int UrgentOrders { get; set; }    // Priority = Urgent
        public int PlannedOrders { get; set; }   // Has a planning row, not yet active/urgent/completed
        public decimal OnTimeDeliveryRate { get; set; }
        public int LowStockCount { get; set; }
        public int TotalMachines { get; set; }
        public int MachinesActive { get; set; }
        public string UPSStatus { get; set; }
    }
}

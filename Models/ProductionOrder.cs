// /Models/ProductionOrder.cs
using System;

namespace RPACProductionPlanner.Models
{
    public class ProductionOrder
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public string SalesOrderNo { get; set; } // SO
        public string CustomerName { get; set; }
        public string ProductName { get; set; } // FG Description / Item Name
        public string Department { get; set; }
        public decimal Quantity { get; set; } // Order Qty
        public string UnitOfMeasure { get; set; }
        public string Status { get; set; } // Current Stage
        public string Priority { get; set; }
        
        // Fast React Specific Fields
        public string CurrentStage { get; set; }
        public string DocketUpdate { get; set; }
        public string PlanningReceivedDate { get; set; }
        public string OrderType { get; set; }
        public decimal Delivered { get; set; }
        public decimal Balance { get; set; }
        public string FGDescription { get; set; }
        public string DeliveryDateCS { get; set; }
        public string Time { get; set; }
        public string Value { get; set; }
        public string PrePressStatus { get; set; } // Added back for compatibility

        public DateTime? PlannedStart { get; set; }
        public DateTime? PlannedEnd { get; set; }
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }
        public int? MachineId { get; set; }
        public string MachineName { get; set; }
        public int? AssignedEmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? OrderedDate { get; set; }
        public string Notes { get; set; }
    }
}

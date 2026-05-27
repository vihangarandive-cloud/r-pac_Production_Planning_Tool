// /Models/InventoryItem.cs
using System;

namespace RPACProductionPlanner.Models
{
    public class InventoryItem
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Category { get; set; }
        public decimal QuantityOnHand { get; set; }
        public decimal ReorderLevel { get; set; }
        public string UnitOfMeasure { get; set; }
        public DateTime LastUpdated { get; set; }
        public int? SupplierId { get; set; }
        public bool IsLowStock { get; set; }
    }
}

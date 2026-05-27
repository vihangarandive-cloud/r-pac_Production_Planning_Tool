// /Models/BillOfMaterial.cs
namespace RPACProductionPlanner.Models
{
    public class BillOfMaterial
    {
        public int BOMId { get; set; }
        public string ProductName { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public decimal QuantityRequired { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal Shortage { get; set; }
        public decimal TotalRequired { get; set; }
        public decimal QuantityOnHand { get; set; }
    }
}

// /Repositories/IInventoryRepository.cs
using System.Collections.Generic;
using RPACProductionPlanner.Models;

namespace RPACProductionPlanner.Repositories
{
    public interface IInventoryRepository
    {
        IEnumerable<InventoryItem> GetAll();
        System.Threading.Tasks.Task<IEnumerable<InventoryItem>> GetAllAsync();
        IEnumerable<BillOfMaterial> CheckBOM(string productName, decimal quantity);
        IEnumerable<AlertNotification> GetAlerts(int? userId = null);
        void MarkAlertRead(int alertId);
        void UpdateStock(int itemId, decimal quantity);
        void Add(InventoryItem item);
        IEnumerable<InventoryItem> GetLowStockItems();
    }
}

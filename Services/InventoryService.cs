// /Services/InventoryService.cs
using System.Collections.Generic;
using System.Linq;
using RPACProductionPlanner.Models;
using RPACProductionPlanner.Repositories;

namespace RPACProductionPlanner.Services
{
    public class InventoryService
    {
        private readonly IInventoryRepository _inventoryRepo;

        public InventoryService(IInventoryRepository inventoryRepo)
        {
            _inventoryRepo = inventoryRepo;
        }

        public IEnumerable<InventoryItem> GetLowStockItems()
        {
            return _inventoryRepo.GetAll().Where(i => i.QuantityOnHand <= i.ReorderLevel);
        }
    }
}

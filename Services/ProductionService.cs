// /Services/ProductionService.cs
using System;
using System.Collections.Generic;
using RPACProductionPlanner.Models;
using RPACProductionPlanner.Repositories;

namespace RPACProductionPlanner.Services
{
    public class ProductionService
    {
        private readonly IProductionOrderRepository _orderRepo;
        private readonly IInventoryRepository _inventoryRepo;

        public ProductionService(IProductionOrderRepository orderRepo, IInventoryRepository inventoryRepo)
        {
            _orderRepo = orderRepo;
            _inventoryRepo = inventoryRepo;
        }

        public ServiceResult CreateOrder(ProductionOrder order)
        {
            // 1. BOM Availability Check
            var shortages = _inventoryRepo.CheckBOM(order.ProductName, order.Quantity);
            foreach (var item in shortages)
            {
                if (item.Shortage > 0)
                {
                    return new ServiceResult(false, string.Format("Insufficient inventory for {0}. Required: {1}, Available: {2}", item.ItemName, item.TotalRequired, item.QuantityOnHand));
                }
            }

            // 2. Persistence
            order.CreatedAt = DateTime.Now;
            order.PrePressStatus = "Pending"; // Initialize pre-press workflow
            _orderRepo.Create(order);
            return new ServiceResult(true, "Order created successfully.");
        }
    }
}

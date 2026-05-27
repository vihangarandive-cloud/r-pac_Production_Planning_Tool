// /Repositories/InventoryRepository.cs
using System.Collections.Generic;
using System.Data;
using Dapper;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Models;

namespace RPACProductionPlanner.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        public async System.Threading.Tasks.Task<IEnumerable<InventoryItem>> GetAllAsync()
        {
            var cacheKey = "Inventory_All";
            if (System.Web.HttpContext.Current != null)
            {
                var cached = System.Web.HttpContext.Current.Cache[cacheKey] as IEnumerable<InventoryItem>;
                if (cached != null) return cached;
            }

            using (var conn = MSSQLHelper.GetConnection())
            {
                var items = await conn.QueryAsync<InventoryItem>("SELECT * FROM InventoryItems WITH (NOLOCK)");
                if (System.Web.HttpContext.Current != null)
                {
                    System.Web.HttpContext.Current.Cache.Insert(cacheKey, items, null, System.DateTime.Now.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration);
                }
                return items;
            }
        }

        public IEnumerable<InventoryItem> GetAll()
        {
            var cacheKey = "Inventory_All";
            if (System.Web.HttpContext.Current != null)
            {
                var cached = System.Web.HttpContext.Current.Cache[cacheKey] as IEnumerable<InventoryItem>;
                if (cached != null) return cached;
            }

            using (var conn = MSSQLHelper.GetConnection())
            {
                var items = conn.Query<InventoryItem>("SELECT * FROM InventoryItems WITH (NOLOCK)");
                if (System.Web.HttpContext.Current != null)
                {
                    System.Web.HttpContext.Current.Cache.Insert(cacheKey, items, null, System.DateTime.Now.AddMinutes(5), System.Web.Caching.Cache.NoSlidingExpiration);
                }
                return items;
            }
        }

        public IEnumerable<BillOfMaterial> CheckBOM(string productName, decimal quantity)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = "SELECT * FROM BillOfMaterials WHERE ProductName = @ProductName";
                return conn.Query<BillOfMaterial>(sql, new { ProductName = productName });
            }
        }

        public IEnumerable<AlertNotification> GetAlerts(int? userId = null)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                return conn.Query<AlertNotification>("SELECT * FROM AlertNotifications WITH (NOLOCK) WHERE IsRead = 0 AND CreatedAt >= DATEADD(day, -30, GETDATE())");
            }
        }

        public void MarkAlertRead(int alertId)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                conn.Execute("UPDATE AlertNotifications SET IsRead = 1 WHERE AlertId = @AlertId", new { AlertId = alertId });
            }
        }

        public void UpdateStock(int itemId, decimal quantity)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                conn.Execute("UPDATE InventoryItems SET QuantityOnHand = @Quantity, LastUpdated = GETDATE() WHERE ItemId = @ItemId", new { ItemId = itemId, Quantity = quantity });
            }
        }

        public void Add(InventoryItem item)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"INSERT INTO InventoryItems (ItemCode, ItemName, Category, QuantityOnHand, ReorderLevel, UnitOfMeasure, LastUpdated) 
                               VALUES (@ItemCode, @ItemName, @Category, @QuantityOnHand, @ReorderLevel, @UnitOfMeasure, GETDATE())";
                conn.Execute(sql, item);
            }
        }

        public IEnumerable<InventoryItem> GetLowStockItems()
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                return conn.Query<InventoryItem>("SELECT * FROM InventoryItems WHERE QuantityOnHand <= ReorderLevel");
            }
        }
    }
}

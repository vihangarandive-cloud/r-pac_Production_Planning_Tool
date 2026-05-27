// /Services/NotificationService.cs
using RPACProductionPlanner.Models;
using RPACProductionPlanner.Repositories;
using Dapper;

namespace RPACProductionPlanner.Services
{
    public class NotificationService
    {
        private readonly IInventoryRepository _inventoryRepo;
        private readonly INotificationRepository _notificationRepo;

        public NotificationService(IInventoryRepository inventoryRepo)
        {
            _inventoryRepo = inventoryRepo;
            _notificationRepo = new NotificationRepository();
        }

        private static System.DateTime? _lastRunTime;
        private static bool _isRunning = false;
        private static readonly object _lock = new object();

        public void ProcessAlerts()
        {
            // 1. Thread-safe check to prevent overlapping runs and respect cooldown
            lock (_lock)
            {
                if (_isRunning) return;
                
                if (_lastRunTime.HasValue && System.DateTime.Now.Subtract(_lastRunTime.Value).TotalMinutes < 15)
                {
                    return;
                }
                
                // Mark as running and update last run time immediately to block other threads
                _isRunning = true;
                _lastRunTime = System.DateTime.Now;
            }

            try
            {
                // 2. Perform DB work (Outside the lock to keep it non-blocking)
                var lowStockItems = _inventoryRepo.GetLowStockItems();
                using (var conn = Helpers.MSSQLHelper.GetConnection())
                {
                    foreach (var item in lowStockItems)
                    {
                        string checkSql = "SELECT TOP 1 1 FROM AlertNotifications WITH (NOLOCK) WHERE AlertType = 'Low Stock' AND RelatedId = @RelatedId AND IsRead = 0";
                        if (conn.ExecuteScalar<int?>(checkSql, new { RelatedId = item.ItemId }) == null)
                        {
                            string message = $"Low stock alert: {item.ItemName} ({item.ItemCode}) is at {item.QuantityOnHand} {item.UnitOfMeasure}.";
                            _notificationRepo.CreateAlert("Low Stock", message, item.ItemId);
                        }
                    }

                    string sql = @"SELECT * FROM ProductionOrders WITH (NOLOCK)
                                   WHERE PlannedEnd < GETDATE() 
                                   AND Status NOT IN ('Completed', 'Cancelled')";
                    var overdueOrders = conn.Query<Models.ProductionOrder>(sql);

                    foreach (var order in overdueOrders)
                    {
                        string checkSql = "SELECT TOP 1 1 FROM AlertNotifications WITH (NOLOCK) WHERE AlertType = 'Overdue' AND RelatedId = @RelatedId AND IsRead = 0";
                        if (conn.ExecuteScalar<int?>(checkSql, new { RelatedId = order.OrderId }) == null)
                        {
                            string message = $"Overdue order alert: {order.OrderCode} ({order.ProductName}) was planned to end on {order.PlannedEnd:MMM dd, HH:mm}.";
                            _notificationRepo.CreateAlert("Overdue", message, order.OrderId);
                        }
                    }
                }
            }
            catch
            {
                // Silent fail for background tasks
            }
            finally
            {
                // 3. Always reset the running flag
                lock (_lock)
                {
                    _isRunning = false;
                }
            }
        }
    }
}

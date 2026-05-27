using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Models;

namespace RPACProductionPlanner.Repositories
{
    public interface INotificationRepository
    {
        void CreateAlert(string alertType, string message, int? relatedId = null);
        IEnumerable<AlertNotification> GetUnread();
        void MarkAsRead(int alertId);
    }

    public class NotificationRepository : INotificationRepository
    {
        public void CreateAlert(string alertType, string message, int? relatedId = null)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"INSERT INTO AlertNotifications (AlertType, Message, RelatedId, IsRead, CreatedAt)
                               VALUES (@AlertType, @Message, @RelatedId, 0, GETDATE())";
                conn.Execute(sql, new { AlertType = alertType, Message = message, RelatedId = relatedId });
            }
        }

        public IEnumerable<AlertNotification> GetUnread()
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = "SELECT * FROM AlertNotifications WHERE IsRead = 0 ORDER BY CreatedAt DESC";
                return conn.Query<AlertNotification>(sql);
            }
        }

        public void MarkAsRead(int alertId)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = "UPDATE AlertNotifications SET IsRead = 1 WHERE AlertId = @AlertId";
                conn.Execute(sql, new { AlertId = alertId });
            }
        }
    }
}

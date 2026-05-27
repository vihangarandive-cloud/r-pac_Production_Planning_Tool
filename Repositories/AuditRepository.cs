using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Models;

namespace RPACProductionPlanner.Repositories
{
    public interface IAuditRepository
    {
        void Log(int? userId, string action, string table, string recordId, string ipAddress = null);
        System.Threading.Tasks.Task LogAsync(int? userId, string action, string table, string recordId, string ipAddress = null);
        IEnumerable<AuditLog> GetLatest(int count = 50);
        System.Threading.Tasks.Task<IEnumerable<AuditLog>> GetLatestAsync(int count = 50);
    }

    public class AuditRepository : IAuditRepository
    {
        public void Log(int? userId, string action, string table, string recordId, string ipAddress = null)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"INSERT INTO AuditLog (UserId, Action, TableAffected, RecordId, Timestamp, IPAddress)
                               VALUES (@UserId, @Action, @Table, @RecordId, GETDATE(), @IPAddress)";
                conn.Execute(sql, new { UserId = userId, Action = action, Table = table, RecordId = recordId, IPAddress = ipAddress });
            }
        }

        public async System.Threading.Tasks.Task LogAsync(int? userId, string action, string table, string recordId, string ipAddress = null)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"INSERT INTO AuditLog (UserId, Action, TableAffected, RecordId, Timestamp, IPAddress)
                               VALUES (@UserId, @Action, @Table, @RecordId, GETDATE(), @IPAddress)";
                await conn.ExecuteAsync(sql, new { UserId = userId, Action = action, Table = table, RecordId = recordId, IPAddress = ipAddress });
            }
        }

        public IEnumerable<AuditLog> GetLatest(int count = 50)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"SELECT TOP (@Count) a.*, u.FullName as UserFullName 
                               FROM AuditLog a WITH (NOLOCK)
                               LEFT JOIN Users u WITH (NOLOCK) ON a.UserId = u.UserId
                               ORDER BY a.Timestamp DESC";
                return conn.Query<AuditLog>(sql, new { Count = count });
            }
        }

        public async System.Threading.Tasks.Task<IEnumerable<AuditLog>> GetLatestAsync(int count = 50)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"SELECT TOP (@Count) a.*, u.FullName as UserFullName 
                               FROM AuditLog a WITH (NOLOCK)
                               LEFT JOIN Users u WITH (NOLOCK) ON a.UserId = u.UserId
                               ORDER BY a.Timestamp DESC";
                return await conn.QueryAsync<AuditLog>(sql, new { Count = count });
            }
        }
    }
}

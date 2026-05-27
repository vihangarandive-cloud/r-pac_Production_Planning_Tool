// /Repositories/UserRepository.cs
using System.Data;
using Dapper;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Models;

namespace RPACProductionPlanner.Repositories
{
    public class UserRepository : IUserRepository
    {
        public UserAccount ValidateUser(string username, string passwordHash)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                // Check if username and password match
                string sql = @"SELECT u.*, 
                               CASE WHEN r.RoleName IS NULL OR r.RoleName = '' THEN 'Standard User' ELSE r.RoleName END as RoleName 
                               FROM Users u WITH (NOLOCK) 
                               LEFT JOIN Roles r ON u.RoleId = r.RoleId 
                               WHERE u.Username = @Username AND u.PasswordHash = @PasswordHash";
                var user = conn.QueryFirstOrDefault<UserAccount>(sql, new { Username = username, PasswordHash = passwordHash });
                return user;
            }
        }
        public UserAccount GetUserByUsername(string username)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = "SELECT * FROM Users WITH (NOLOCK) WHERE Username = @Username";
                var user = conn.QueryFirstOrDefault<UserAccount>(sql, new { Username = username });
                if (user != null)
                {
                    user.RoleName = conn.ExecuteScalar<string>("SELECT RoleName FROM Roles WITH (NOLOCK) WHERE RoleId = @RoleId", new { RoleId = user.RoleId });
                }
                return user;
            }
        }
        public async System.Threading.Tasks.Task<UserAccount> GetUserByUsernameAsync(string username)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = "SELECT * FROM Users WITH (NOLOCK) WHERE Username = @Username";
                var user = await conn.QueryFirstOrDefaultAsync<UserAccount>(sql, new { Username = username });
                if (user != null)
                {
                    user.RoleName = await conn.ExecuteScalarAsync<string>("SELECT RoleName FROM Roles WITH (NOLOCK) WHERE RoleId = @RoleId", new { RoleId = user.RoleId });
                }
                return user;
            }
        }
        public System.Collections.Generic.IEnumerable<UserAccount> GetAllUsers()
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                // Explicitly select columns to avoid shadowing and ensure RoleName is correct
                string sql = @"SELECT 
                                 u.UserId, u.FullName, u.Username, u.RoleId, u.IsActive,
                                 CASE WHEN r.RoleName IS NULL OR r.RoleName = '' THEN 'Standard User' ELSE r.RoleName END as RoleName 
                               FROM Users u WITH (NOLOCK)
                               LEFT JOIN Roles r WITH (NOLOCK) ON u.RoleId = r.RoleId";
                return conn.Query<UserAccount>(sql);
            }
        }
        public void AddUser(UserAccount user)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"INSERT INTO Users (FullName, Username, PasswordHash, RoleId, IsActive) 
                               VALUES (@FullName, @Username, @PasswordHash, @RoleId, 1)";
                conn.Execute(sql, user);
            }
        }

        public void UpdateUser(UserAccount user)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"UPDATE Users SET FullName = @FullName, RoleId = @RoleId WHERE Username = @Username";
                conn.Execute(sql, user);
            }
        }

        public void ToggleStatus(string username)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = "UPDATE Users SET IsActive = 1 - IsActive WHERE Username = @Username";
                conn.Execute(sql, new { Username = username });
            }
        }
    }
}

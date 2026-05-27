using System.Collections.Generic;
using RPACProductionPlanner.Models;

namespace RPACProductionPlanner.Repositories
{
    public interface IUserRepository
    {
        UserAccount ValidateUser(string username, string passwordHash);
        UserAccount GetUserByUsername(string username);
        System.Threading.Tasks.Task<UserAccount> GetUserByUsernameAsync(string username);
        IEnumerable<UserAccount> GetAllUsers();
        void AddUser(UserAccount user);
        void UpdateUser(UserAccount user);
        void ToggleStatus(string username);
    }
}

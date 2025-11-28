using System.Collections.Generic;
using System.Threading.Tasks;
using ADManagementApp.Models;

namespace ADManagementApp.Services
{
    public interface IADService
    {
        // Connection
        Task<bool> TestConnectionAsync(string domain, string username, string password);
        void SetCredentials(string domain, string username, string password, string defaultOU);
        
        // Users
        Task<List<ADUser>> GetAllUsersAsync(string searchTerm = "");
        Task<ADUser?> GetUserAsync(string username);
        Task<bool> CreateUserAsync(ADUser user, string password);
        Task<bool> UpdateUserAsync(ADUser user);
        Task<bool> DeleteUserAsync(string username);
        Task<bool> EnableUserAsync(string username);
        Task<bool> DisableUserAsync(string username);
        Task<bool> ResetPasswordAsync(string username, string newPassword, bool mustChangePassword);
        Task<bool> UnlockAccountAsync(string username);
        
        // Groups
        Task<List<ADGroup>> GetAllGroupsAsync(string searchTerm = "");
        Task<ADGroup?> GetGroupAsync(string groupname);
        Task<bool> CreateGroupAsync(ADGroup group);
        Task<bool> DeleteGroupAsync(string groupname);
        Task<bool> AddUserToGroupAsync(string groupname, string username);
        Task<bool> RemoveUserFromGroupAsync(string groupname, string username);
        
        // Statistics
        Task<DomainStats> GetDomainStatsAsync();
    }
}
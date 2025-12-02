using System.Collections.Generic;
using System.Threading.Tasks;
using ADManagementApp.Models;

namespace ADManagementApp.Services
{
    /// <summary>
    /// Interface for Active Directory operations.
    /// Provides abstraction for AD user and group management, connection testing, and domain statistics.
    /// </summary>
    public interface IADService
    {
        #region Connection Management

        /// <summary>
        /// Tests the Active Directory connection with the specified credentials.
        /// </summary>
        /// <param name="domain">The AD domain name (e.g., "yourdomain.local")</param>
        /// <param name="username">The username for authentication (e.g., "DOMAIN\Administrator")</param>
        /// <param name="password">The password for authentication</param>
        /// <returns>True if connection is successful; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when domain, username, or password is null.</exception>
        Task<bool> TestConnectionAsync(string domain, string username, string password);

        /// <summary>
        /// Sets the credentials for subsequent Active Directory operations.
        /// Credentials are securely stored using Windows DPAPI.
        /// </summary>
        /// <param name="domain">The AD domain name</param>
        /// <param name="username">The username for authentication</param>
        /// <param name="password">The password for authentication</param>
        /// <param name="defaultOU">Optional default Organizational Unit path</param>
        void SetCredentials(string domain, string username, string password, string defaultOU);

        #endregion

        #region User Management

        /// <summary>
        /// Retrieves all users from Active Directory, optionally filtered by search term.
        /// </summary>
        /// <param name="searchTerm">Optional search term to filter users by name or email (case-insensitive)</param>
        /// <returns>A list of all users matching the search criteria</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when no connection is established</exception>
        Task<List<ADUser>> GetAllUsersAsync(string searchTerm = "");

        /// <summary>
        /// Retrieves a specific user from Active Directory by username.
        /// </summary>
        /// <param name="username">The SAM account name or UPN of the user (e.g., "DOMAIN\jdoe" or "jdoe@yourdomain.local")</param>
        /// <returns>The user object if found; otherwise, null.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when username is null or empty</exception>
        Task<ADUser?> GetUserAsync(string username);

        /// <summary>
        /// Creates a new user in Active Directory.
        /// </summary>
        /// <param name="user">The user object containing all user properties</param>
        /// <param name="password">The initial password for the user</param>
        /// <returns>True if user creation is successful; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when user or password is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when user already exists or creation fails</exception>
        Task<bool> CreateUserAsync(ADUser user, string password);

        /// <summary>
        /// Updates an existing user in Active Directory.
        /// </summary>
        /// <param name="user">The user object with updated properties</param>
        /// <returns>True if update is successful; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when user is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when user is not found</exception>
        Task<bool> UpdateUserAsync(ADUser user);

        /// <summary>
        /// Deletes a user from Active Directory.
        /// </summary>
        /// <param name="username">The username (SAM account name) of the user to delete</param>
        /// <returns>True if deletion is successful; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when username is null or empty</exception>
        Task<bool> DeleteUserAsync(string username);

        /// <summary>
        /// Enables a disabled user account in Active Directory.
        /// </summary>
        /// <param name="username">The username of the user to enable</param>
        /// <returns>True if the operation is successful; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when username is null or empty</exception>
        Task<bool> EnableUserAsync(string username);

        /// <summary>
        /// Disables an active user account in Active Directory.
        /// </summary>
        /// <param name="username">The username of the user to disable</param>
        /// <returns>True if the operation is successful; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when username is null or empty</exception>
        Task<bool> DisableUserAsync(string username);

        /// <summary>
        /// Resets a user's password in Active Directory.
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="newPassword">The new password to set</param>
        /// <param name="mustChangePassword">If true, user must change password on next logon</param>
        /// <returns>True if password reset is successful; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when username or newPassword is null or empty</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when password doesn't meet complexity requirements</exception>
        Task<bool> ResetPasswordAsync(string username, string newPassword, bool mustChangePassword);

        /// <summary>
        /// Unlocks a locked user account in Active Directory.
        /// </summary>
        /// <param name="username">The username of the user to unlock</param>
        /// <returns>True if unlock is successful; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when username is null or empty</exception>
        Task<bool> UnlockAccountAsync(string username);

        #endregion

        #region Group Management

        /// <summary>
        /// Retrieves all groups from Active Directory, optionally filtered by search term.
        /// </summary>
        /// <param name="searchTerm">Optional search term to filter groups by name (case-insensitive)</param>
        /// <returns>A list of all groups matching the search criteria</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when no connection is established</exception>
        Task<List<ADGroup>> GetAllGroupsAsync(string searchTerm = "");

        /// <summary>
        /// Retrieves a specific group from Active Directory by group name.
        /// </summary>
        /// <param name="groupname">The name of the group (e.g., "Domain Admins")</param>
        /// <returns>The group object if found; otherwise, null.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when groupname is null or empty</exception>
        Task<ADGroup?> GetGroupAsync(string groupname);

        /// <summary>
        /// Creates a new group in Active Directory.
        /// </summary>
        /// <param name="group">The group object containing group properties</param>
        /// <returns>True if group creation is successful; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when group is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when group already exists</exception>
        Task<bool> CreateGroupAsync(ADGroup group);

        /// <summary>
        /// Deletes a group from Active Directory.
        /// </summary>
        /// <param name="groupname">The name of the group to delete</param>
        /// <returns>True if deletion is successful; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when groupname is null or empty</exception>
        Task<bool> DeleteGroupAsync(string groupname);

        /// <summary>
        /// Adds a user to a group in Active Directory.
        /// </summary>
        /// <param name="groupname">The name of the group</param>
        /// <param name="username">The username to add to the group</param>
        /// <returns>True if the operation is successful; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when groupname or username is null or empty</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when user is already in the group</exception>
        Task<bool> AddUserToGroupAsync(string groupname, string username);

        /// <summary>
        /// Removes a user from a group in Active Directory.
        /// </summary>
        /// <param name="groupname">The name of the group</param>
        /// <param name="username">The username to remove from the group</param>
        /// <returns>True if the operation is successful; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when groupname or username is null or empty</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when user is not in the group</exception>
        Task<bool> RemoveUserFromGroupAsync(string groupname, string username);

        #endregion

        #region Statistics and Information

        /// <summary>
        /// Retrieves statistics about the Active Directory domain.
        /// Includes user counts, group counts, disabled users, and other domain-level metrics.
        /// </summary>
        /// <returns>A DomainStats object containing domain statistics</returns>
        /// <exception cref="System.InvalidOperationException">Thrown when no connection is established or data cannot be retrieved</exception>
        Task<DomainStats> GetDomainStatsAsync();

        #endregion
    }
}

using System.Threading.Tasks;
using ADManagementApp.Models;

namespace ADManagementApp.Services
{
    /// <summary>
    /// Service for securely managing credentials
    /// Uses Windows Credential Manager for secure storage
    /// PRODUCTION-READY interface with validation and expiration support
    /// </summary>
    public interface ICredentialService
    {
        /// <summary>
        /// Save credentials securely to Windows Credential Manager
        /// </summary>
        /// <param name="domain">Active Directory domain</param>
        /// <param name="username">Username (can include domain prefix)</param>
        /// <param name="password">Password (will be encrypted by Windows)</param>
        /// <returns>Task representing the async operation</returns>
        Task SaveCredentialsAsync(string domain, string username, string password);

        /// <summary>
        /// Retrieve credentials securely from Windows Credential Manager
        /// </summary>
        /// <returns>SecureCredentials object or null if not found</returns>
        Task<SecureCredentials?> GetCredentialsAsync();

        /// <summary>
        /// Delete stored credentials from Windows Credential Manager
        /// </summary>
        Task DeleteCredentialsAsync();

        /// <summary>
        /// Check if credentials are currently stored
        /// </summary>
        /// <returns>True if credentials exist, false otherwise</returns>
        Task<bool> HasStoredCredentialsAsync();

        /// <summary>
        /// Validate stored credentials by testing AD connection
        /// </summary>
        /// <param name="adService">AD service to test connection with</param>
        /// <returns>True if credentials are valid and can connect</returns>
        Task<bool> ValidateStoredCredentialsAsync(IADService adService);
    }
}

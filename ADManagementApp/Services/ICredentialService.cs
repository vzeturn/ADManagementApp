using System.Threading.Tasks;

namespace ADManagementApp.Services
{
    /// <summary>
    /// Service for securely managing credentials
    /// Uses Windows Credential Manager for secure storage
    /// </summary>
    public interface ICredentialService
    {
        /// <summary>
        /// Save credentials securely
        /// </summary>
        Task SaveCredentialsAsync(string domain, string username, string password);

        /// <summary>
        /// Retrieve credentials securely
        /// </summary>
        Task<(string Domain, string Username, string Password)?> GetCredentialsAsync();

        /// <summary>
        /// Delete stored credentials
        /// </summary>
        Task DeleteCredentialsAsync();

        /// <summary>
        /// Check if credentials are stored
        /// </summary>
        Task<bool> HasStoredCredentialsAsync();
    }
}
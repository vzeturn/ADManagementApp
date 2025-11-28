using System.Threading.Tasks;

namespace ADManagementApp.Services
{
    /// <summary>
    /// Service for displaying dialogs without coupling ViewModels to WPF
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows an error message
        /// </summary>
        void ShowError(string message, string title = "Error");

        /// <summary>
        /// Shows a success message
        /// </summary>
        void ShowSuccess(string message, string title = "Success");

        /// <summary>
        /// Shows an information message
        /// </summary>
        void ShowInformation(string message, string title = "Information");

        /// <summary>
        /// Shows a warning message
        /// </summary>
        void ShowWarning(string message, string title = "Warning");

        /// <summary>
        /// Shows a confirmation dialog
        /// </summary>
        Task<bool> ShowConfirmationAsync(string message, string title = "Confirm");

        /// <summary>
        /// Shows a dialog for creating a new user
        /// </summary>
        Task<(bool Success, Models.ADUser? User, string Password)> ShowCreateUserDialogAsync();

        /// <summary>
        /// Shows a dialog for editing a user
        /// </summary>
        Task<(bool Success, Models.ADUser? User)> ShowEditUserDialogAsync(Models.ADUser user);

        /// <summary>
        /// Shows a dialog for resetting password
        /// </summary>
        Task<(bool Success, string Password, bool MustChange)> ShowResetPasswordDialogAsync(string displayName);

        /// <summary>
        /// Shows user details dialog
        /// </summary>
        void ShowUserDetails(Models.ADUser user);

        /// <summary>
        /// Shows a dialog for creating a new group
        /// </summary>
        Task<(bool Success, Models.ADGroup? Group)> ShowCreateGroupDialogAsync();

        /// <summary>
        /// Shows group members dialog
        /// </summary>
        void ShowGroupMembers(Models.ADGroup group);

        /// <summary>
        /// Shows add member to group dialog
        /// </summary>
        Task<(bool Success, string? Username)> ShowAddMemberDialogAsync(Models.ADGroup group);
    }
}
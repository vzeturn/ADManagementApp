using System.Threading.Tasks;
using System.Windows;
using ADManagementApp.Models;
using ADManagementApp.Views;

namespace ADManagementApp.Services
{
    /// <summary>
    /// WPF implementation of IDialogService
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly IADService _adService;

        public DialogService(IADService adService)
        {
            _adService = adService;
        }

        public void ShowError(string message, string title = "Error")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public void ShowSuccess(string message, string title = "Success")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public void ShowInformation(string message, string title = "Information")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public void ShowWarning(string message, string title = "Warning")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }

        public Task<bool> ShowConfirmationAsync(string message, string title = "Confirm")
        {
            return Task.FromResult(
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                    return result == MessageBoxResult.Yes;
                })
            );
        }

        public Task<(bool Success, ADUser? User, string Password)> ShowCreateUserDialogAsync()
        {
            return Task.Run(() =>
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    var dialog = new CreateUserDialog();
                    var result = dialog.ShowDialog() == true;
                    return (result, dialog.NewUser, dialog.Password);
                });
            });
        }

        public Task<(bool Success, ADUser? User)> ShowEditUserDialogAsync(ADUser user)
        {
            return Task.Run(() =>
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    var dialog = new EditUserDialog(user);
                    var result = dialog.ShowDialog() == true;
                    return (result, dialog.EditedUser);
                });
            });
        }

        public Task<(bool Success, string Password, bool MustChange)> ShowResetPasswordDialogAsync(string displayName)
        {
            return Task.Run(() =>
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    var dialog = new ResetPasswordDialog(displayName);
                    var result = dialog.ShowDialog() == true;
                    return (result, dialog.NewPassword, dialog.MustChangePassword);
                });
            });
        }

        public void ShowUserDetails(ADUser user)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new UserDetailsDialog(user);
                dialog.ShowDialog();
            });
        }

        public Task<(bool Success, ADGroup? Group)> ShowCreateGroupDialogAsync()
        {
            return Task.Run(() =>
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    var dialog = new CreateGroupDialog();
                    var result = dialog.ShowDialog() == true;
                    return (result, dialog.NewGroup);
                });
            });
        }

        public void ShowGroupMembers(ADGroup group)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dialog = new GroupMembersDialog(group, _adService);
                dialog.ShowDialog();
            });
        }

        public Task<(bool Success, string? Username)> ShowAddMemberDialogAsync(ADGroup group)
        {
            return Task.Run(() =>
            {
                return Application.Current.Dispatcher.Invoke(() =>
                {
                    var dialog = new AddMemberDialog(group, _adService);
                    var result = dialog.ShowDialog() == true;
                    return (result, dialog.SelectedUsername);
                });
            });
        }
    }
}
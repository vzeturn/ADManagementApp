using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ADManagementApp.Helpers;
using ADManagementApp.Models;
using ADManagementApp.Services;

namespace ADManagementApp.ViewModels
{
    public class UserManagementViewModel : BaseViewModel
    {
        private readonly IADService _adService;
        private ObservableCollection<ADUser> _users = new();
        private ADUser? _selectedUser;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public UserManagementViewModel(IADService adService)
        {
            _adService = adService;
            
            LoadUsersCommand = new AsyncRelayCommand(async _ => await LoadUsersAsync());
            SearchCommand = new AsyncRelayCommand(async _ => await SearchUsersAsync());
            CreateUserCommand = new RelayCommand(_ => ShowCreateUserDialog());
            EditUserCommand = new RelayCommand(_ => ShowEditUserDialog(), _ => SelectedUser != null);
            DeleteUserCommand = new AsyncRelayCommand(async _ => await DeleteUserAsync(), _ => SelectedUser != null);
            EnableUserCommand = new AsyncRelayCommand(async _ => await EnableUserAsync(), _ => SelectedUser != null);
            DisableUserCommand = new AsyncRelayCommand(async _ => await DisableUserAsync(), _ => SelectedUser != null);
            ResetPasswordCommand = new RelayCommand(_ => ShowResetPasswordDialog(), _ => SelectedUser != null);
            ViewDetailsCommand = new RelayCommand(_ => ShowUserDetails(), _ => SelectedUser != null);
            UnlockAccountCommand = new AsyncRelayCommand(async _ => await UnlockAccountAsync(), _ => SelectedUser != null);
        }

        public ObservableCollection<ADUser> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public ADUser? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoadUsersCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand EnableUserCommand { get; }
        public ICommand DisableUserCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand UnlockAccountCommand { get; }

        public async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                var users = await _adService.GetAllUsersAsync();
                Users = new ObservableCollection<ADUser>(users);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchUsersAsync()
        {
            try
            {
                IsLoading = true;
                var users = await _adService.GetAllUsersAsync(SearchText);
                Users = new ObservableCollection<ADUser>(users);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching users: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowCreateUserDialog()
        {
            var dialog = new Views.CreateUserDialog();
            if (dialog.ShowDialog() == true && dialog.NewUser != null)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var success = await _adService.CreateUserAsync(dialog.NewUser, dialog.Password);
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (success)
                            {
                                MessageBox.Show("User created successfully!", "Success", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadUsersAsync().Wait();
                            }
                            else
                            {
                                MessageBox.Show("Failed to create user.", "Error", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Error creating user: {ex.Message}", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }
        }

        private void ShowEditUserDialog()
        {
            if (SelectedUser == null) return;

            var dialog = new Views.EditUserDialog(SelectedUser);
            if (dialog.ShowDialog() == true && dialog.EditedUser != null)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var success = await _adService.UpdateUserAsync(dialog.EditedUser);
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (success)
                            {
                                MessageBox.Show("User updated successfully!", "Success", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadUsersAsync().Wait();
                            }
                            else
                            {
                                MessageBox.Show("Failed to update user.", "Error", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Error updating user: {ex.Message}", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }
        }

        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete user '{SelectedUser.DisplayName}'?\n\nThis action cannot be undone!",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _adService.DeleteUserAsync(SelectedUser.SamAccountName);
                    
                    if (success)
                    {
                        MessageBox.Show("User deleted successfully!", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadUsersAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete user.", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task EnableUserAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                var success = await _adService.EnableUserAsync(SelectedUser.SamAccountName);
                
                if (success)
                {
                    MessageBox.Show("User enabled successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadUsersAsync();
                }
                else
                {
                    MessageBox.Show("Failed to enable user.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error enabling user: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DisableUserAsync()
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to disable user '{SelectedUser.DisplayName}'?",
                "Confirm Disable",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _adService.DisableUserAsync(SelectedUser.SamAccountName);
                    
                    if (success)
                    {
                        MessageBox.Show("User disabled successfully!", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadUsersAsync();
                    }
                    else
                    {
                        MessageBox.Show("Failed to disable user.", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error disabling user: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowResetPasswordDialog()
        {
            if (SelectedUser == null) return;

            var dialog = new Views.ResetPasswordDialog(SelectedUser.DisplayName);
            if (dialog.ShowDialog() == true)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var success = await _adService.ResetPasswordAsync(
                            SelectedUser.SamAccountName, 
                            dialog.NewPassword, 
                            dialog.MustChangePassword);
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (success)
                            {
                                MessageBox.Show("Password reset successfully!", "Success", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Failed to reset password.", "Error", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Error resetting password: {ex.Message}", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }
        }

        private void ShowUserDetails()
        {
            if (SelectedUser == null) return;

            var dialog = new Views.UserDetailsDialog(SelectedUser);
            dialog.ShowDialog();
        }

        private async Task UnlockAccountAsync()
        {
            if (SelectedUser == null) return;

            try
            {
                var success = await _adService.UnlockAccountAsync(SelectedUser.SamAccountName);
                
                if (success)
                {
                    MessageBox.Show("Account unlocked successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to unlock account.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error unlocking account: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
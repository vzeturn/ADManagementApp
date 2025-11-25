using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ADManagementApp.Helpers;
using ADManagementApp.Models;
using ADManagementApp.Services;

namespace ADManagementApp.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IADService _adService;
        private DomainStats? _stats;
        private bool _isLoading;

        public DashboardViewModel(IADService adService)
        {
            _adService = adService;

            // Initialize commands
            LoadDataCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
            RefreshCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
            CreateUserCommand = new RelayCommand(_ => CreateUser());
            CreateGroupCommand = new RelayCommand(_ => CreateGroup());
        }

        #region Properties

        public DomainStats? Stats
        {
            get => _stats;
            set => SetProperty(ref _stats, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        #endregion

        #region Commands

        public ICommand LoadDataCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand CreateGroupCommand { get; }

        #endregion

        #region Methods

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                // Load domain statistics
                Stats = await _adService.GetDomainStatsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // Set default stats if error
                Stats = new DomainStats
                {
                    TotalUsers = 0,
                    EnabledUsers = 0,
                    DisabledUsers = 0,
                    TotalGroups = 0,
                    DomainName = "Error loading domain",
                    DomainController = "N/A"
                };
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CreateUser()
        {
            try
            {
                var dialog = new Views.CreateUserDialog();
                if (dialog.ShowDialog() == true && dialog.NewUser != null)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            var success = await _adService.CreateUserAsync(dialog.NewUser, dialog.Password);

                            Application.Current.Dispatcher.Invoke(async () =>
                            {
                                if (success)
                                {
                                    MessageBox.Show("User created successfully!", "Success",
                                        MessageBoxButton.OK, MessageBoxImage.Information);

                                    // Refresh dashboard data
                                    await LoadDataAsync();
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening create user dialog: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateGroup()
        {
            try
            {
                var dialog = new Views.CreateGroupDialog();
                if (dialog.ShowDialog() == true && dialog.NewGroup != null)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            var success = await _adService.CreateGroupAsync(dialog.NewGroup);

                            Application.Current.Dispatcher.Invoke(async () =>
                            {
                                if (success)
                                {
                                    MessageBox.Show("Group created successfully!", "Success",
                                        MessageBoxButton.OK, MessageBoxImage.Information);

                                    // Refresh dashboard data
                                    await LoadDataAsync();
                                }
                                else
                                {
                                    MessageBox.Show("Failed to create group.", "Error",
                                        MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show($"Error creating group: {ex.Message}", "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening create group dialog: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
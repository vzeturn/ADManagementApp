using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ADManagementApp.Helpers;
using ADManagementApp.Models;
using ADManagementApp.Services;

namespace ADManagementApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IADService _adService;
        private object? _currentView;
        private DomainStats? _stats;
        private bool _isConnected;
        private string _statusMessage = "Not connected";

        public MainViewModel(IADService adService)
        {
            _adService = adService;

            // Initialize ViewModels
            DashboardViewModel = new DashboardViewModel(_adService);
            UserManagementViewModel = new UserManagementViewModel(_adService);
            GroupManagementViewModel = new GroupManagementViewModel(_adService);

            // Initialize commands
            NavigateToDashboardCommand = new RelayCommand(_ => NavigateToDashboard());
            NavigateToUsersCommand = new RelayCommand(_ => NavigateToUsers());
            NavigateToGroupsCommand = new RelayCommand(_ => NavigateToGroups());
            NavigateToSettingsCommand = new RelayCommand(_ => ShowSettings());
            RefreshCommand = new AsyncRelayCommand(async _ => await RefreshDataAsync());
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());

            // Set initial view to Dashboard
            CurrentView = DashboardViewModel;

            // Auto-connect using credentials from appsettings.json
            Task.Run(async () => await AutoConnectAsync());
        }

        public DashboardViewModel DashboardViewModel { get; }
        public UserManagementViewModel UserManagementViewModel { get; }
        public GroupManagementViewModel GroupManagementViewModel { get; }

        public object? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public DomainStats? Stats
        {
            get => _stats;
            set => SetProperty(ref _stats, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToUsersCommand { get; }
        public ICommand NavigateToGroupsCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ExitCommand { get; }

        private void NavigateToDashboard()
        {
            CurrentView = DashboardViewModel;
        }

        private void NavigateToUsers()
        {
            CurrentView = UserManagementViewModel;
        }

        private void NavigateToGroups()
        {
            CurrentView = GroupManagementViewModel;
        }

        private async Task AutoConnectAsync()
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    StatusMessage = "Connecting...";
                });

                // Try to connect using credentials from appsettings.json
                // You should load these from configuration
                var domain = "corp.haier.com";
                var username = "240156260";
                var password = "";

                var connected = await _adService.TestConnectionAsync(domain, username, password);

                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    if (connected)
                    {
                        _adService.SetCredentials(domain, username, password);
                        IsConnected = true;
                        StatusMessage = $"Connected to {domain}";

                        // Load initial data
                        await RefreshDataAsync();
                    }
                    else
                    {
                        IsConnected = false;
                        StatusMessage = "Connection failed - Click Settings to configure";
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsConnected = false;
                    StatusMessage = $"Error: {ex.Message}";
                });
            }
        }

        private async Task RefreshDataAsync()
        {
            if (!IsConnected)
                return;

            try
            {
                StatusMessage = "Refreshing...";

                Stats = await _adService.GetDomainStatsAsync();

                // Refresh current view
                if (CurrentView == DashboardViewModel)
                    await DashboardViewModel.LoadDataAsync();
                else if (CurrentView == UserManagementViewModel)
                    await UserManagementViewModel.LoadUsersAsync();
                else if (CurrentView == GroupManagementViewModel)
                    await GroupManagementViewModel.LoadGroupsAsync();

                StatusMessage = $"Connected to {Stats?.DomainName ?? "domain"}";
            }
            catch (Exception ex)
            {
                StatusMessage = "Error refreshing data";
                MessageBox.Show($"Error refreshing data: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowSettings()
        {
            var settingsWindow = new Views.SettingsWindow(_adService);
            if (settingsWindow.ShowDialog() == true)
            {
                // Reload connection with new settings
                Task.Run(async () => await AutoConnectAsync());
            }
        }
    }

}
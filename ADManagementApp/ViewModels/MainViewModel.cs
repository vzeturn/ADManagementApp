using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ADManagementApp.Models;
using ADManagementApp.Services;

namespace ADManagementApp.ViewModels
{
    /// <summary>
    /// Main ViewModel for the application
    /// Handles navigation, connection state, and overall app state
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly IConfiguration _configuration;
        private readonly IADService _adService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<MainViewModel> _logger;

        private DomainStats? _stats;
        private bool _isConnected;
        private string _statusMessage = "Not connected";
        private bool _isInitialized;

        public MainViewModel(
            IConfiguration configuration,
            IADService adService,
            INavigationService navigationService,
            IDialogService dialogService,
            DashboardViewModel dashboardViewModel,
            UserManagementViewModel userManagementViewModel,
            GroupManagementViewModel groupManagementViewModel,
            ILogger<MainViewModel> logger)
        {
            _configuration = configuration;
            _adService = adService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _logger = logger;

            // Store ViewModels
            DashboardViewModel = dashboardViewModel;
            UserManagementViewModel = userManagementViewModel;
            GroupManagementViewModel = groupManagementViewModel;

            // Initialize commands
            NavigateToDashboardCommand = new RelayCommand(NavigateToDashboard, () => !IsBusy);
            NavigateToUsersCommand = new RelayCommand(NavigateToUsers, () => !IsBusy);
            NavigateToGroupsCommand = new RelayCommand(NavigateToGroups, () => !IsBusy);
            NavigateToSettingsCommand = new RelayCommand(ShowSettings, () => !IsBusy);
            RefreshCommand = new AsyncRelayCommand(RefreshDataAsync, () => !IsBusy);

            // Subscribe to navigation changes
            _navigationService.NavigationChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(CurrentView));
            };

            // Start initialization
            _ = InitializeAsync();
        }

        #region Properties

        public DashboardViewModel DashboardViewModel { get; }
        public UserManagementViewModel UserManagementViewModel { get; }
        public GroupManagementViewModel GroupManagementViewModel { get; }

        public object? CurrentView => _navigationService.CurrentViewModel;

        public DomainStats? Stats
        {
            get => _stats;
            set => SetProperty(ref _stats, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (SetProperty(ref _isConnected, value))
                {
                    OnPropertyChanged(nameof(ConnectionStatusText));
                    OnPropertyChanged(nameof(ConnectionStatusColor));
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string ConnectionStatusText => IsConnected ? "Connected" : "Disconnected";
        public string ConnectionStatusColor => IsConnected ? "#4CAF50" : "#F44336";

        #endregion

        #region Commands

        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToUsersCommand { get; }
        public ICommand NavigateToGroupsCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Initialize the application - only test connection, no auto-loading
        /// </summary>
        private async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                SetBusy(true, "Connecting to Active Directory...");
                _logger.LogInformation("Starting application initialization");

                // Only connect to Active Directory - don't load data yet
                await ConnectToActiveDirectoryAsync();

                // Navigate to dashboard - user will manually load data when needed
                _navigationService.NavigateTo(DashboardViewModel);

                _isInitialized = true;
                _logger.LogInformation("Application initialization completed successfully. Connection status: {IsConnected}", IsConnected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during application initialization");
                _dialogService.ShowError($"Initialization error: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        /// <summary>
        /// Connect to Active Directory using configuration
        /// </summary>
        private async Task ConnectToActiveDirectoryAsync()
        {
            try
            {
                SetBusy(true, "Connecting to Active Directory...");
                StatusMessage = "Connecting...";

                var domain = _configuration["ActiveDirectory:Domain"];
                var username = _configuration["ActiveDirectory:AdminUsername"];
                var password = _configuration["ActiveDirectory:AdminPassword"];
                var defaultOU = _configuration["ActiveDirectory:DefaultOU"];

                if (string.IsNullOrWhiteSpace(domain) ||
                    string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Active Directory credentials not configured");
                    IsConnected = false;
                    StatusMessage = "Configuration missing - Please update Settings";
                    _dialogService.ShowWarning(
                        "Active Directory credentials are not configured in appsettings.json",
                        "Configuration Required");
                    return;
                }

                // Test connection
                var connected = await _adService.TestConnectionAsync(domain, username, password);

                if (connected)
                {
                    _adService.SetCredentials(domain, username, password, defaultOU ?? "");
                    IsConnected = true;
                    StatusMessage = $"Connected to {domain}";
                    _logger.LogInformation("Successfully connected to Active Directory: {Domain}", domain);

                    // Load domain statistics
                    Stats = await _adService.GetDomainStatsAsync();
                }
                else
                {
                    IsConnected = false;
                    StatusMessage = "Connection failed";
                    _logger.LogWarning("Failed to connect to Active Directory");
                    _dialogService.ShowError("Failed to connect to Active Directory. Please check your credentials.");
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                StatusMessage = "Connection error";
                _logger.LogError(ex, "Error connecting to Active Directory");
                _dialogService.ShowError($"Error connecting to Active Directory: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        /// <summary>
        /// Refresh data for current view
        /// </summary>
        private async Task RefreshDataAsync()
        {
            if (!IsConnected)
            {
                _dialogService.ShowWarning("Not connected to Active Directory");
                return;
            }

            try
            {
                SetBusy(true, "Refreshing data...");
                _logger.LogInformation("Refreshing data for current view");

                // Refresh stats
                Stats = await _adService.GetDomainStatsAsync();

                // Refresh current view
                if (CurrentView == DashboardViewModel)
                    await DashboardViewModel.LoadDataAsync();
                else if (CurrentView == UserManagementViewModel)
                    await UserManagementViewModel.LoadUsersAsync();
                else if (CurrentView == GroupManagementViewModel)
                    await GroupManagementViewModel.LoadGroupsAsync();

                _logger.LogInformation("Data refresh completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing data");
                _dialogService.ShowError($"Error refreshing data: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void NavigateToDashboard()
        {
            _logger.LogDebug("Navigating to Dashboard");
            _navigationService.NavigateTo(DashboardViewModel);
            // User will manually refresh data when needed
        }

        private void NavigateToUsers()
        {
            _logger.LogDebug("Navigating to User Management");
            _navigationService.NavigateTo(UserManagementViewModel);
            // User will manually load/search users when needed
        }

        private void NavigateToGroups()
        {
            _logger.LogDebug("Navigating to Group Management");
            _navigationService.NavigateTo(GroupManagementViewModel);
            // User will manually load/search groups when needed
        }

        private void ShowSettings()
        {
            _logger.LogDebug("Opening Settings");
            _dialogService.ShowInformation("Settings functionality will be implemented soon", "Settings");
        }

        #endregion
    }
}
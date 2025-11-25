using System;
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
            NavigateToDashboardCommand = new RelayCommand(_ => CurrentView = DashboardViewModel);
            NavigateToUsersCommand = new RelayCommand(_ => CurrentView = UserManagementViewModel);
            NavigateToGroupsCommand = new RelayCommand(_ => CurrentView = GroupManagementViewModel);
            NavigateToSettingsCommand = new RelayCommand(_ => ShowSettings());
            RefreshCommand = new AsyncRelayCommand(async _ => await RefreshDataAsync());
            ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());

            // Set initial view
            CurrentView = DashboardViewModel;
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

        public async void InitializeConnection(string domain, string username, string password)
        {
            try
            {
                StatusMessage = "Connecting...";
                
                var connected = await _adService.TestConnectionAsync(domain, username, password);
                
                if (connected)
                {
                    _adService.SetCredentials(domain, username, password);
                    IsConnected = true;
                    StatusMessage = $"Connected to {domain}";
                    
                    await RefreshDataAsync();
                }
                else
                {
                    IsConnected = false;
                    StatusMessage = "Connection failed";
                    MessageBox.Show("Failed to connect to domain. Please check your credentials.", 
                        "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                StatusMessage = "Error connecting";
                MessageBox.Show($"Error: {ex.Message}", "Connection Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task RefreshDataAsync()
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
                StatusMessage = "Settings updated";
            }
        }
    }

    public class DashboardViewModel : BaseViewModel
    {
        private readonly IADService _adService;
        private DomainStats? _stats;

        public DashboardViewModel(IADService adService)
        {
            _adService = adService;
            LoadDataCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
        }

        public DomainStats? Stats
        {
            get => _stats;
            set => SetProperty(ref _stats, value);
        }

        public ICommand LoadDataCommand { get; }

        public async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                Stats = await _adService.GetDomainStatsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
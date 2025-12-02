using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ADManagementApp.Models;
using ADManagementApp.Services;

namespace ADManagementApp.ViewModels
{
    /// <summary>
    /// ViewModel for Dashboard view
    /// Displays domain statistics and quick actions
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IADService _adService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<DashboardViewModel> _logger;

        private DomainStats? _stats;

        public DashboardViewModel(
            IADService adService,
            IDialogService dialogService,
            ILogger<DashboardViewModel> logger)
        {
            _adService = adService;
            _dialogService = dialogService;
            _logger = logger;

            // Initialize commands
            RefreshCommand = new AsyncRelayCommand(LoadDataAsync, () => !IsBusy);
            CreateUserCommand = new AsyncRelayCommand(CreateUserAsync, () => !IsBusy);
            CreateGroupCommand = new AsyncRelayCommand(CreateGroupAsync, () => !IsBusy);
        }

        #region Properties

        public DomainStats? Stats
        {
            get => _stats;
            set => SetProperty(ref _stats, value);
        }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand CreateGroupCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Load dashboard data
        /// </summary>
        public async Task LoadDataAsync()
        {
            try
            {
                SetBusy(true, "Loading dashboard data...");
                _logger.LogInformation("Loading dashboard data");

                // Load domain statistics
                Stats = await _adService.GetDomainStatsAsync();

                _logger.LogInformation("Dashboard data loaded successfully. Users: {TotalUsers}, Groups: {TotalGroups}",
                    Stats.TotalUsers, Stats.TotalGroups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                _dialogService.ShowError($"Error loading dashboard data: {ex.Message}");

                // Set default stats on error
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
                SetBusy(false);
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        private async Task CreateUserAsync()
        {
            try
            {
                _logger.LogInformation("Opening create user dialog");

                var (success, user, password) = await _dialogService.ShowCreateUserDialogAsync();

                if (!success || user == null)
                {
                    _logger.LogDebug("User creation cancelled");
                    return;
                }

                SetBusy(true, "Creating user...");
                _logger.LogInformation("Creating user: {Username}", user.SamAccountName);

                var created = await _adService.CreateUserAsync(user, password);

                if (created)
                {
                    _logger.LogInformation("User created successfully: {Username}", user.SamAccountName);
                    _dialogService.ShowSuccess($"User '{user.DisplayName}' created successfully!");

                    // Refresh dashboard data
                    await LoadDataAsync();
                }
                else
                {
                    _logger.LogWarning("Failed to create user: {Username}", user.SamAccountName);
                    _dialogService.ShowError("Failed to create user.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                _dialogService.ShowError($"Error creating user: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        /// <summary>
        /// Create a new group
        /// </summary>
        private async Task CreateGroupAsync()
        {
            try
            {
                _logger.LogInformation("Opening create group dialog");

                var (success, group) = await _dialogService.ShowCreateGroupDialogAsync();

                if (!success || group == null)
                {
                    _logger.LogDebug("Group creation cancelled");
                    return;
                }

                SetBusy(true, "Creating group...");
                _logger.LogInformation("Creating group: {GroupName}", group.SamAccountName);

                var created = await _adService.CreateGroupAsync(group);

                if (created)
                {
                    _logger.LogInformation("Group created successfully: {GroupName}", group.SamAccountName);
                    _dialogService.ShowSuccess($"Group '{group.Name}' created successfully!");

                    // Refresh dashboard data
                    await LoadDataAsync();
                }
                else
                {
                    _logger.LogWarning("Failed to create group: {GroupName}", group.SamAccountName);
                    _dialogService.ShowError("Failed to create group.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group");
                _dialogService.ShowError($"Error creating group: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        #endregion
    }
}

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ADManagementApp.Models;
using ADManagementApp.Services;

namespace ADManagementApp.ViewModels
{
    /// <summary>
    /// ViewModel for Group Management view
    /// Handles all group-related operations
    /// </summary>
    public class GroupManagementViewModel : BaseViewModel
    {
        private readonly IADService _adService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<GroupManagementViewModel> _logger;

        private ObservableCollection<ADGroup> _groups = new();
        private ADGroup? _selectedGroup;
        private string _searchText = string.Empty;

        public GroupManagementViewModel(
            IADService adService,
            IDialogService dialogService,
            ILogger<GroupManagementViewModel> logger)
        {
            _adService = adService;
            _dialogService = dialogService;
            _logger = logger;

            // Initialize commands
            LoadGroupsCommand = new AsyncRelayCommand(LoadGroupsAsync, () => !IsBusy);
            SearchCommand = new AsyncRelayCommand(SearchGroupsAsync, () => !IsBusy);
            CreateGroupCommand = new AsyncRelayCommand(CreateGroupAsync, () => !IsBusy);
            DeleteGroupCommand = new AsyncRelayCommand(DeleteGroupAsync, () => SelectedGroup != null && !IsBusy);
            ViewMembersCommand = new RelayCommand(ViewMembers, () => SelectedGroup != null);
            AddMemberCommand = new AsyncRelayCommand(AddMemberAsync, () => SelectedGroup != null && !IsBusy);
        }

        #region Properties

        public ObservableCollection<ADGroup> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        public ADGroup? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                {
                    // Notify command can execute changed
                    (DeleteGroupCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                    (ViewMembersCommand as RelayCommand)?.NotifyCanExecuteChanged();
                    (AddMemberCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        #endregion

        #region Commands

        public ICommand LoadGroupsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CreateGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand ViewMembersCommand { get; }
        public ICommand AddMemberCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Load all groups from Active Directory
        /// </summary>
        public async Task LoadGroupsAsync()
        {
            try
            {
                SetBusy(true, "Loading groups...");
                _logger.LogInformation("Loading all groups");

                var groups = await _adService.GetAllGroupsAsync();
                Groups = new ObservableCollection<ADGroup>(groups);

                _logger.LogInformation("Loaded {Count} groups", Groups.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading groups");
                _dialogService.ShowError($"Error loading groups: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        /// <summary>
        /// Search groups by text
        /// </summary>
        private async Task SearchGroupsAsync()
        {
            try
            {
                SetBusy(true, "Searching groups...");
                _logger.LogInformation("Searching groups with term: {SearchTerm}", SearchText);

                var groups = await _adService.GetAllGroupsAsync(SearchText);
                Groups = new ObservableCollection<ADGroup>(groups);

                _logger.LogInformation("Found {Count} groups matching '{SearchTerm}'", Groups.Count, SearchText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching groups");
                _dialogService.ShowError($"Error searching groups: {ex.Message}");
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

                SetBusy(true, $"Creating group '{group.SamAccountName}'...");
                _logger.LogInformation("Creating group: {GroupName}", group.SamAccountName);

                var created = await _adService.CreateGroupAsync(group);

                if (created)
                {
                    _logger.LogInformation("Group created successfully: {GroupName}", group.SamAccountName);
                    _dialogService.ShowSuccess($"Group '{group.Name}' created successfully!");
                    await LoadGroupsAsync();
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

        /// <summary>
        /// Delete selected group
        /// </summary>
        private async Task DeleteGroupAsync()
        {
            if (SelectedGroup == null) return;

            try
            {
                _logger.LogInformation("Requesting confirmation to delete group: {GroupName}", SelectedGroup.SamAccountName);

                var confirmed = await _dialogService.ShowConfirmationAsync(
                    $"Are you sure you want to delete group '{SelectedGroup.Name}'?\n\nThis action cannot be undone!",
                    "Confirm Delete");

                if (!confirmed)
                {
                    _logger.LogDebug("Group deletion cancelled");
                    return;
                }

                SetBusy(true, $"Deleting group '{SelectedGroup.SamAccountName}'...");
                _logger.LogInformation("Deleting group: {GroupName}", SelectedGroup.SamAccountName);

                var deleted = await _adService.DeleteGroupAsync(SelectedGroup.SamAccountName);

                if (deleted)
                {
                    _logger.LogInformation("Group deleted successfully: {GroupName}", SelectedGroup.SamAccountName);
                    _dialogService.ShowSuccess("Group deleted successfully!");
                    await LoadGroupsAsync();
                }
                else
                {
                    _logger.LogWarning("Failed to delete group: {GroupName}", SelectedGroup.SamAccountName);
                    _dialogService.ShowError("Failed to delete group.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting group");
                _dialogService.ShowError($"Error deleting group: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        /// <summary>
        /// View members of selected group
        /// </summary>
        private void ViewMembers()
        {
            if (SelectedGroup == null) return;

            _logger.LogInformation("Showing members for group: {GroupName}", SelectedGroup.SamAccountName);
            _dialogService.ShowGroupMembers(SelectedGroup);
        }

        /// <summary>
        /// Add member to selected group
        /// </summary>
        private async Task AddMemberAsync()
        {
            if (SelectedGroup == null) return;

            try
            {
                _logger.LogInformation("Opening add member dialog for group: {GroupName}", SelectedGroup.SamAccountName);

                var (success, username) = await _dialogService.ShowAddMemberDialogAsync(SelectedGroup);

                if (success && !string.IsNullOrEmpty(username))
                {
                    _logger.LogInformation("User {Username} added to group {GroupName}", username, SelectedGroup.SamAccountName);
                    await LoadGroupsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding member to group");
                _dialogService.ShowError($"Error adding member: {ex.Message}");
            }
        }

        #endregion
    }
}
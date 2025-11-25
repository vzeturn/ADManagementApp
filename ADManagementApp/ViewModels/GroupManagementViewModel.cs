using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ADManagementApp.Helpers;
using ADManagementApp.Models;
using ADManagementApp.Services;

namespace ADManagementApp.ViewModels
{
    public class GroupManagementViewModel : BaseViewModel
    {
        private readonly IADService _adService;
        private ObservableCollection<ADGroup> _groups = new();
        private ADGroup? _selectedGroup;
        private string _searchText = string.Empty;
        private bool _isLoading;

        public GroupManagementViewModel(IADService adService)
        {
            _adService = adService;
            
            LoadGroupsCommand = new AsyncRelayCommand(async _ => await LoadGroupsAsync());
            SearchCommand = new AsyncRelayCommand(async _ => await SearchGroupsAsync());
            CreateGroupCommand = new RelayCommand(_ => ShowCreateGroupDialog());
            DeleteGroupCommand = new AsyncRelayCommand(async _ => await DeleteGroupAsync(), _ => SelectedGroup != null);
            ViewMembersCommand = new RelayCommand(_ => ShowGroupMembers(), _ => SelectedGroup != null);
            AddMemberCommand = new RelayCommand(_ => ShowAddMemberDialog(), _ => SelectedGroup != null);
        }

        public ObservableCollection<ADGroup> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        public ADGroup? SelectedGroup
        {
            get => _selectedGroup;
            set => SetProperty(ref _selectedGroup, value);
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

        public ICommand LoadGroupsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CreateGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand ViewMembersCommand { get; }
        public ICommand AddMemberCommand { get; }

        public async Task LoadGroupsAsync()
        {
            try
            {
                IsLoading = true;
                var groups = await _adService.GetAllGroupsAsync();
                Groups = new ObservableCollection<ADGroup>(groups);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading groups: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchGroupsAsync()
        {
            try
            {
                IsLoading = true;
                var groups = await _adService.GetAllGroupsAsync(SearchText);
                Groups = new ObservableCollection<ADGroup>(groups);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching groups: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowCreateGroupDialog()
        {
            var dialog = new Views.CreateGroupDialog();
            if (dialog.ShowDialog() == true && dialog.NewGroup != null)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var success = await _adService.CreateGroupAsync(dialog.NewGroup);
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (success)
                            {
                                MessageBox.Show("Group created successfully!", "Success", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadGroupsAsync().Wait();
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show($"Error: {ex.Message}", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        });
                    }
                });
            }
        }

        private async Task DeleteGroupAsync()
        {
            if (SelectedGroup == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete group '{SelectedGroup.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var success = await _adService.DeleteGroupAsync(SelectedGroup.SamAccountName);
                    if (success)
                    {
                        MessageBox.Show("Group deleted successfully!", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadGroupsAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ShowGroupMembers()
        {
            if (SelectedGroup == null) return;
            var dialog = new Views.GroupMembersDialog(SelectedGroup, _adService);
            dialog.ShowDialog();
        }

        private void ShowAddMemberDialog()
        {
            if (SelectedGroup == null) return;
            var dialog = new Views.AddMemberDialog(SelectedGroup, _adService);
            if (dialog.ShowDialog() == true)
            {
                LoadGroupsAsync().Wait();
            }
        }
    }
}
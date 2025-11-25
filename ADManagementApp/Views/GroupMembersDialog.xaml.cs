using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ADManagementApp.Models;
using ADManagementApp.Services;

namespace ADManagementApp.Views
{
    public partial class GroupMembersDialog : Window
    {
        private readonly ADGroup _group;
        private readonly IADService _adService;

        public GroupMembersDialog(ADGroup group, IADService adService)
        {
            InitializeComponent();

            _group = group;
            _adService = adService;

            GroupNameTextBlock.Text = $"Group: {group.Name} ({group.MemberCount} members)";

            LoadMembers();
        }

        private async void LoadMembers()
        {
            try
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                EmptyStateBorder.Visibility = Visibility.Collapsed;

                // Reload group with members
                var groupWithMembers = await _adService.GetGroupAsync(_group.SamAccountName);

                if (groupWithMembers != null && groupWithMembers.Members.Count > 0)
                {
                    MembersDataGrid.ItemsSource = groupWithMembers.Members;
                    GroupNameTextBlock.Text = $"Group: {groupWithMembers.Name} ({groupWithMembers.MemberCount} members)";
                }
                else
                {
                    MembersDataGrid.ItemsSource = null;
                    EmptyStateBorder.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading group members: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private async void RemoveMemberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string memberSamAccountName)
            {
                var member = (MembersDataGrid.ItemsSource as System.Collections.Generic.List<ADGroupMember>)?
                    .FirstOrDefault(m => m.SamAccountName == memberSamAccountName);

                if (member == null)
                    return;

                var result = MessageBox.Show(
                    $"Are you sure you want to remove '{member.DisplayName}' from this group?",
                    "Confirm Remove",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        LoadingOverlay.Visibility = Visibility.Visible;

                        var success = await _adService.RemoveUserFromGroupAsync(
                            _group.SamAccountName,
                            memberSamAccountName);

                        if (success)
                        {
                            MessageBox.Show($"Successfully removed {member.DisplayName} from the group.",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                            // Reload members
                            LoadMembers();
                        }
                        else
                        {
                            MessageBox.Show("Failed to remove member from group.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error removing member: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        LoadingOverlay.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMembers();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
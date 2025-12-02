using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ADManagementApp.Models;
using ADManagementApp.Services;

namespace ADManagementApp.Views
{
    public partial class AddMemberDialog : Window
    {
        private readonly ADGroup _group;
        private readonly IADService _adService;
        private List<ADUser> _searchResults = new List<ADUser>();

        public string? SelectedUsername { get; private set; }

        public AddMemberDialog(ADGroup group, IADService adService)
        {
            InitializeComponent();

            _group = group;
            _adService = adService;

            GroupNameTextBlock.Text = $"Group: {group.Name}";

            // Wire up selection changed event
            UsersDataGrid.SelectionChanged += UsersDataGrid_SelectionChanged;

            // Set focus to search box
            Loaded += (s, e) => SearchTextBox.Focus();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var searchTerm = SearchTextBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                MessageBox.Show("Please enter a search term.", "Search Required",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Show loading overlay
                LoadingOverlay.Visibility = Visibility.Visible;
                EmptyStateBorder.Visibility = Visibility.Collapsed;
                SearchButton.IsEnabled = false;

                // Search for users
                var allUsers = await _adService.GetAllUsersAsync(searchTerm);

                // Filter out users who are already members
                _searchResults = allUsers
                    .Where(u => !_group.Members.Any(m =>
                        m.SamAccountName.Equals(u.SamAccountName, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                // Update DataGrid
                UsersDataGrid.ItemsSource = _searchResults;

                if (_searchResults.Count == 0)
                {
                    EmptyStateBorder.Visibility = Visibility.Visible;
                    MessageBox.Show("No users found matching your search criteria, or all matching users are already members of this group.",
                        "No Results", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching for users: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                EmptyStateBorder.Visibility = Visibility.Visible;
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                SearchButton.IsEnabled = true;
            }
        }

        private void UsersDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            AddButton.IsEnabled = UsersDataGrid.SelectedItem != null;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is ADUser selectedUser)
            {
                try
                {
                    AddButton.IsEnabled = false;
                    LoadingOverlay.Visibility = Visibility.Visible;

                    var success = await _adService.AddUserToGroupAsync(
                        _group.SamAccountName,
                        selectedUser.SamAccountName);

                    if (success)
                    {
                        SelectedUsername = selectedUser.SamAccountName;
                        MessageBox.Show($"Successfully added {selectedUser.DisplayName} to {_group.Name}!",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show($"Failed to add {selectedUser.DisplayName} to the group.",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding member: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                    AddButton.IsEnabled = true;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

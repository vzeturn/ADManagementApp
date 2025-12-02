using System.Windows;
using ADManagementApp.Models;

namespace ADManagementApp.Views
{
    public partial class CreateGroupDialog : Window
    {
        public ADGroup? NewGroup { get; private set; }

        public CreateGroupDialog()
        {
            InitializeComponent();

            // Set focus to first field
            Loaded += (s, e) => GroupNameTextBox.Focus();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(GroupNameTextBox.Text))
            {
                MessageBox.Show("Please enter a group name.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                GroupNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(SamAccountNameTextBox.Text))
            {
                MessageBox.Show("Please enter a SAM account name.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                SamAccountNameTextBox.Focus();
                return;
            }

            // Validate SAM account name (alphanumeric and some special characters)
            var samAccountName = SamAccountNameTextBox.Text.Trim();
            if (samAccountName.Length > 20)
            {
                MessageBox.Show("SAM account name must be 20 characters or less.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                SamAccountNameTextBox.Focus();
                return;
            }

            // Check for invalid characters in SAM account name
            foreach (char c in samAccountName)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_' && c != '.')
                {
                    MessageBox.Show("SAM account name can only contain letters, numbers, hyphens, underscores, and periods.",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SamAccountNameTextBox.Focus();
                    return;
                }
            }

            // Create the new group object
            NewGroup = new ADGroup
            {
                Name = GroupNameTextBox.Text.Trim(),
                SamAccountName = samAccountName,
                Description = DescriptionTextBox.Text.Trim()
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

using System;
using System.Windows;
using ADManagementApp.Models;

namespace ADManagementApp.Views
{
    public partial class CreateUserDialog : Window
    {
        public ADUser? NewUser { get; private set; }
        public string Password { get; private set; } = string.Empty;

        public CreateUserDialog()
        {
            InitializeComponent();

            // Set focus to first field
            Loaded += (s, e) => UsernameTextBox.Focus();
        }

        private void DisplayNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Auto-generate display name if empty
            if (string.IsNullOrWhiteSpace(DisplayNameTextBox.Text))
            {
                var firstName = FirstNameTextBox.Text?.Trim();
                var lastName = LastNameTextBox.Text?.Trim();

                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    DisplayNameTextBox.Text = $"{firstName} {lastName}";
                }
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Please enter a username.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                MessageBox.Show("Please enter first name.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                FirstNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                MessageBox.Show("Please enter last name.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LastNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(DisplayNameTextBox.Text))
            {
                MessageBox.Show("Please enter display name.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DisplayNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Please enter a password.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Passwords do not match!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ConfirmPasswordBox.Focus();
                return;
            }

            // Validate password strength
            if (PasswordBox.Password.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                PasswordBox.Focus();
                return;
            }

            // Validate username (alphanumeric and some special characters)
            var username = UsernameTextBox.Text.Trim();
            if (username.Length > 20)
            {
                MessageBox.Show("Username must be 20 characters or less.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                UsernameTextBox.Focus();
                return;
            }

            foreach (char c in username)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_' && c != '.')
                {
                    MessageBox.Show("Username can only contain letters, numbers, hyphens, underscores, and periods.",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    UsernameTextBox.Focus();
                    return;
                }
            }

            // Validate email if provided
            if (!string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                if (!EmailTextBox.Text.Contains("@") || !EmailTextBox.Text.Contains("."))
                {
                    MessageBox.Show("Please enter a valid email address.", "Validation Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    EmailTextBox.Focus();
                    return;
                }
            }

            // Create the new user object
            NewUser = new ADUser
            {
                SamAccountName = username,
                GivenName = FirstNameTextBox.Text.Trim(),
                Surname = LastNameTextBox.Text.Trim(),
                DisplayName = DisplayNameTextBox.Text.Trim(),
                EmailAddress = EmailTextBox.Text?.Trim() ?? string.Empty,
                Department = DepartmentTextBox.Text?.Trim() ?? string.Empty,
                Title = TitleTextBox.Text?.Trim() ?? string.Empty,
                PhoneNumber = PhoneTextBox.Text?.Trim() ?? string.Empty,
                Description = DescriptionTextBox.Text?.Trim() ?? string.Empty,
                Enabled = EnabledCheckBox.IsChecked ?? true,
                PasswordNeverExpires = PasswordNeverExpiresCheckBox.IsChecked ?? false,
                UserCannotChangePassword = false
            };

            // Set password
            Password = PasswordBox.Password;

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
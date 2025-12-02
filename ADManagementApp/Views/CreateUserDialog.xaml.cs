using System;
using System.Windows;
using ADManagementApp.Models;
using ADManagementApp.Services;

namespace ADManagementApp.Views
{
    /// <summary>
    /// Dialog for creating a new Active Directory user.
    /// Provides UI for collecting user information and validation before creation.
    /// </summary>
    public partial class CreateUserDialog : Window
    {
        private readonly IValidationService _validationService;

        /// <summary>
        /// Gets the newly created user object if the dialog was completed successfully.
        /// </summary>
        public ADUser? NewUser { get; private set; }

        /// <summary>
        /// Gets the initial password for the new user.
        /// </summary>
        public string Password { get; private set; } = string.Empty;

        // Constructor with ValidationService injection
        public CreateUserDialog(IValidationService validationService)
        {
            InitializeComponent();
            _validationService = validationService;

            // Set focus to first field
            Loaded += (s, e) => UsernameTextBox.Focus();
        }

        // Fallback constructor for design-time
        public CreateUserDialog() : this(new ValidationService())
        {
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
            // Validate username using ValidationService
            var usernameValidation = _validationService.ValidateUsername(UsernameTextBox.Text?.Trim() ?? "");
            if (!usernameValidation.IsValid)
            {
                ShowValidationError("Username Validation", usernameValidation.ErrorMessage);
                UsernameTextBox.Focus();
                return;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                ShowValidationError("Validation Error", "Please enter first name.");
                FirstNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                ShowValidationError("Validation Error", "Please enter last name.");
                LastNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(DisplayNameTextBox.Text))
            {
                ShowValidationError("Validation Error", "Please enter display name.");
                DisplayNameTextBox.Focus();
                return;
            }

            // Validate password using ValidationService
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowValidationError("Validation Error", "Please enter a password.");
                PasswordBox.Focus();
                return;
            }

            var passwordValidation = _validationService.ValidatePassword(PasswordBox.Password);
            if (!passwordValidation.IsValid)
            {
                ShowValidationError("Password Validation", passwordValidation.ErrorMessage);
                PasswordBox.Focus();
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                ShowValidationError("Validation Error", "Passwords do not match!");
                ConfirmPasswordBox.Focus();
                return;
            }

            // Validate email if provided using ValidationService
            if (!string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                var emailValidation = _validationService.ValidateEmail(EmailTextBox.Text.Trim());
                if (!emailValidation.IsValid)
                {
                    ShowValidationError("Email Validation", emailValidation.ErrorMessage);
                    EmailTextBox.Focus();
                    return;
                }
            }

            // Create the new user object
            var user = new ADUser
            {
                SamAccountName = UsernameTextBox?.Text?.Trim() ?? string.Empty,
                GivenName = FirstNameTextBox?.Text?.Trim() ?? string.Empty,
                Surname = LastNameTextBox?.Text?.Trim() ?? string.Empty,
                DisplayName = DisplayNameTextBox?.Text?.Trim() ?? string.Empty,
                EmailAddress = EmailTextBox?.Text?.Trim() ?? string.Empty,
                Department = DepartmentTextBox?.Text?.Trim() ?? string.Empty,
                Title = TitleTextBox?.Text?.Trim() ?? string.Empty,
                PhoneNumber = PhoneTextBox?.Text?.Trim() ?? string.Empty,
                Description = DescriptionTextBox?.Text?.Trim() ?? string.Empty,
                Enabled = EnabledCheckBox?.IsChecked ?? true,
                PasswordNeverExpires = PasswordNeverExpiresCheckBox?.IsChecked ?? false,
                UserCannotChangePassword = false
            };

            // Validate complete user object
            var userValidation = _validationService.ValidateUser(user);
            if (!userValidation.IsValid)
            {
                ShowValidationError("User Validation", userValidation.ErrorMessage);
                return;
            }

            NewUser = user;
            Password = PasswordBox.Password;

            DialogResult = true;
            Close();
        }

        private void ShowValidationError(string title, string message)
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

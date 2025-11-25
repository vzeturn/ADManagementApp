using System.Windows;

namespace ADManagementApp.Views
{
    public partial class ResetPasswordDialog : Window
    {
        public string NewPassword { get; private set; } = string.Empty;
        public bool MustChangePassword { get; private set; }

        public ResetPasswordDialog(string displayName)
        {
            InitializeComponent();
            Title = $"Reset Password - {displayName}";
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Password is required", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Passwords do not match", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PasswordBox.Password.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewPassword = PasswordBox.Password;
            MustChangePassword = MustChangeCheckBox.IsChecked ?? false;
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
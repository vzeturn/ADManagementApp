using System.Windows;
using System.Windows.Controls;
using ADManagementApp.ViewModels;

namespace ADManagementApp.Views
{
    /// <summary>
    /// SettingsView control for managing Active Directory connection settings.
    /// Provides UI for configuring domain, username, and password with connection testing.
    /// Uses SettingsViewModel for business logic and secure credential management.
    /// </summary>
    public partial class SettingsView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the SettingsView class.
        /// </summary>
        public SettingsView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the PasswordBox password changed event.
        /// Updates the ViewModel's Password property since PasswordBox doesn't support data binding for security reasons.
        /// </summary>
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.Password = passwordBox.Password;
            }
        }
    }
}

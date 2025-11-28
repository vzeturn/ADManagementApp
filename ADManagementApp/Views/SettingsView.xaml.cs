using System;
using System.Windows;
using System.Windows.Media;
using ADManagementApp.Services;
using MaterialDesignThemes.Wpf;

namespace ADManagementApp.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly IADService _adService;

        public SettingsWindow(IADService adService)
        {
            InitializeComponent();
            _adService = adService;
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Load settings from configuration
            // This is a simplified version - in production, load from appsettings.json
            DomainTextBox.Text = "yourdomain.local";
            UsernameTextBox.Text = "";
            // Password is not loaded for security reasons
        }

        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DomainTextBox.Text))
            {
                ShowConnectionStatus(false, "Please enter domain name");
                return;
            }

            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                ShowConnectionStatus(false, "Please enter username");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowConnectionStatus(false, "Please enter password");
                return;
            }

            TestConnectionButton.IsEnabled = false;
            ShowConnectionStatus(null, "Testing connection...");

            try
            {
                var result = await _adService.TestConnectionAsync(
                    DomainTextBox.Text,
                    UsernameTextBox.Text,
                    PasswordBox.Password);

                if (result)
                {
                    ShowConnectionStatus(true, "Connection successful!");
                }
                else
                {
                    ShowConnectionStatus(false, "Connection failed. Please check your credentials.");
                }
            }
            catch (Exception ex)
            {
                ShowConnectionStatus(false, $"Error: {ex.Message}");
            }
            finally
            {
                TestConnectionButton.IsEnabled = true;
            }
        }

        private void ShowConnectionStatus(bool? success, string message)
        {
            ConnectionStatusBorder.Visibility = Visibility.Visible;
            ConnectionStatusText.Text = message;

            if (success == true)
            {
                ConnectionStatusBorder.Background = new SolidColorBrush(Color.FromRgb(200, 230, 201));
                ConnectionStatusIcon.Kind = PackIconKind.CheckCircle;
                ConnectionStatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }
            else if (success == false)
            {
                ConnectionStatusBorder.Background = new SolidColorBrush(Color.FromRgb(255, 205, 210));
                ConnectionStatusIcon.Kind = PackIconKind.AlertCircle;
                ConnectionStatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54));
            }
            else
            {
                ConnectionStatusBorder.Background = new SolidColorBrush(Color.FromRgb(187, 222, 251));
                ConnectionStatusIcon.Kind = PackIconKind.Information;
                ConnectionStatusIcon.Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243));
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DomainTextBox.Text))
            {
                MessageBox.Show("Please enter domain name", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Please enter username", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Please enter password", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Save settings
                _adService.SetCredentials(
                    DomainTextBox.Text,
                    UsernameTextBox.Text,
                    PasswordBox.Password,
                    "");

                // In production, save to appsettings.json here
                // SaveToConfiguration();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Helper method to save settings to appsettings.json
        // This would require Microsoft.Extensions.Configuration.Json
        /*
        private void SaveToConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            configuration["ActiveDirectory:Domain"] = DomainTextBox.Text;
            configuration["ActiveDirectory:AdminUsername"] = UsernameTextBox.Text;
            // Note: Don't save passwords in plain text!
        }
        */
    }
}
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ADManagementApp.Services;

namespace ADManagementApp.ViewModels
{
    /// <summary>
    /// ViewModel for Settings view
    /// Handles secure credential management using CredentialService
    /// </summary>
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IConfiguration _configuration;
        private readonly ICredentialService _credentialService;
        private readonly IADService _adService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<SettingsViewModel> _logger;

        private string _domain = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _defaultOU = string.Empty;
        private bool _hasStoredCredentials;
        private DateTime? _credentialsStoredAt;
        private string _connectionStatus = "Not tested";
        private bool _isConnectionSuccessful;

        public SettingsViewModel(
            IConfiguration configuration,
            ICredentialService credentialService,
            IADService adService,
            IDialogService dialogService,
            ILogger<SettingsViewModel> logger)
        {
            _configuration = configuration;
            _credentialService = credentialService;
            _adService = adService;
            _dialogService = dialogService;
            _logger = logger;

            // Initialize commands
            TestConnectionCommand = new AsyncRelayCommand(TestConnectionAsync, () => !IsBusy && CanTestConnection());
            SaveCredentialsCommand = new AsyncRelayCommand(SaveCredentialsAsync, () => !IsBusy && CanSaveCredentials());
            DeleteCredentialsCommand = new AsyncRelayCommand(DeleteCredentialsAsync, () => HasStoredCredentials && !IsBusy);
            LoadStoredCredentialsCommand = new AsyncRelayCommand(LoadStoredCredentialsAsync, () => !IsBusy);
            ValidateStoredCredentialsCommand = new AsyncRelayCommand(ValidateStoredCredentialsAsync, () => HasStoredCredentials && !IsBusy);

            // Load initial data
            _ = InitializeAsync();
        }

        #region Properties

        public string Domain
        {
            get => _domain;
            set
            {
                if (SetProperty(ref _domain, value))
                {
                    (TestConnectionCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                    (SaveCredentialsCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                }
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    (TestConnectionCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                    (SaveCredentialsCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    (TestConnectionCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                    (SaveCredentialsCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                }
            }
        }

        public string DefaultOU
        {
            get => _defaultOU;
            set => SetProperty(ref _defaultOU, value);
        }

        public bool HasStoredCredentials
        {
            get => _hasStoredCredentials;
            set
            {
                if (SetProperty(ref _hasStoredCredentials, value))
                {
                    (DeleteCredentialsCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                    (ValidateStoredCredentialsCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                }
            }
        }

        public DateTime? CredentialsStoredAt
        {
            get => _credentialsStoredAt;
            set => SetProperty(ref _credentialsStoredAt, value);
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        public bool IsConnectionSuccessful
        {
            get => _isConnectionSuccessful;
            set => SetProperty(ref _isConnectionSuccessful, value);
        }

        #endregion

        #region Commands

        public ICommand TestConnectionCommand { get; }
        public ICommand SaveCredentialsCommand { get; }
        public ICommand DeleteCredentialsCommand { get; }
        public ICommand LoadStoredCredentialsCommand { get; }
        public ICommand ValidateStoredCredentialsCommand { get; }

        #endregion

        #region Methods

        private async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing Settings");

                // Load configuration (non-sensitive data only)
                Domain = _configuration["ActiveDirectory:Domain"] ?? string.Empty;
                DefaultOU = _configuration["ActiveDirectory:DefaultOU"] ?? string.Empty;

                // Check if credentials are stored
                HasStoredCredentials = await _credentialService.HasStoredCredentialsAsync();

                if (HasStoredCredentials)
                {
                    var credentials = await _credentialService.GetCredentialsAsync();
                    if (credentials != null)
                    {
                        CredentialsStoredAt = credentials.StoredAt;
                        _logger.LogInformation("Found stored credentials from {StoredAt}", credentials.StoredAt);

                        // Optionally auto-validate on load
                        _dialogService.ShowInformation(
                            $"Stored credentials found for domain: {credentials.Domain}\nStored at: {credentials.StoredAt:g}\n\nClick 'Validate Stored Credentials' to test them.",
                            "Credentials Found");
                    }
                }
                else
                {
                    _logger.LogInformation("No stored credentials found");
                    _dialogService.ShowWarning(
                        "No stored credentials found. Please enter your Active Directory credentials and click 'Save Credentials' to store them securely.",
                        "Setup Required");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing settings");
                _dialogService.ShowError($"Error loading settings: {ex.Message}");
            }
        }

        private bool CanTestConnection()
        {
            return !string.IsNullOrWhiteSpace(Domain) &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password);
        }

        private bool CanSaveCredentials()
        {
            return CanTestConnection();
        }

        private async Task TestConnectionAsync()
        {
            try
            {
                SetBusy(true, "Testing connection...");
                ConnectionStatus = "Testing...";
                IsConnectionSuccessful = false;

                _logger.LogInformation("Testing connection to domain: {Domain} as {Username}", Domain, Username);

                var result = await _adService.TestConnectionAsync(Domain, Username, Password);

                if (result)
                {
                    ConnectionStatus = "✓ Connection successful!";
                    IsConnectionSuccessful = true;
                    _logger.LogInformation("Connection test successful");
                    _dialogService.ShowSuccess("Connection successful! You can now save these credentials.");
                }
                else
                {
                    ConnectionStatus = "✗ Connection failed";
                    IsConnectionSuccessful = false;
                    _logger.LogWarning("Connection test failed");
                    _dialogService.ShowError("Connection failed. Please check your credentials and try again.");
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"✗ Error: {ex.Message}";
                IsConnectionSuccessful = false;
                _logger.LogError(ex, "Error testing connection");
                _dialogService.ShowError($"Connection error: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task SaveCredentialsAsync()
        {
            try
            {
                // Test connection first
                if (!IsConnectionSuccessful)
                {
                    var testFirst = await _dialogService.ShowConfirmationAsync(
                        "Connection has not been tested successfully. Do you want to test it before saving?",
                        "Test Connection First?");

                    if (testFirst)
                    {
                        await TestConnectionAsync();
                        if (!IsConnectionSuccessful)
                        {
                            _dialogService.ShowWarning("Connection test failed. Credentials not saved.");
                            return;
                        }
                    }
                }

                SetBusy(true, "Saving credentials securely...");
                _logger.LogInformation("Saving credentials for domain: {Domain}, user: {Username}", Domain, Username);

                // Save to Windows Credential Manager
                await _credentialService.SaveCredentialsAsync(Domain, Username, Password);

                // Apply to AD Service
                _adService.SetCredentials(Domain, Username, Password, DefaultOU);

                HasStoredCredentials = true;
                CredentialsStoredAt = DateTime.Now;

                _logger.LogInformation("Credentials saved successfully");
                _dialogService.ShowSuccess("Credentials saved securely to Windows Credential Manager!");

                // Clear password field for security
                Password = string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving credentials");
                _dialogService.ShowError($"Error saving credentials: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task DeleteCredentialsAsync()
        {
            try
            {
                var confirmed = await _dialogService.ShowConfirmationAsync(
                    "Are you sure you want to delete the stored credentials?\n\nYou will need to enter them again next time.",
                    "Confirm Delete");

                if (!confirmed)
                    return;

                SetBusy(true, "Deleting credentials...");
                _logger.LogInformation("Deleting stored credentials");

                await _credentialService.DeleteCredentialsAsync();

                HasStoredCredentials = false;
                CredentialsStoredAt = null;
                ConnectionStatus = "Credentials deleted";
                IsConnectionSuccessful = false;

                _logger.LogInformation("Credentials deleted successfully");
                _dialogService.ShowSuccess("Credentials deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting credentials");
                _dialogService.ShowError($"Error deleting credentials: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task LoadStoredCredentialsAsync()
        {
            try
            {
                SetBusy(true, "Loading stored credentials...");
                _logger.LogInformation("Loading stored credentials");

                var credentials = await _credentialService.GetCredentialsAsync();

                if (credentials == null)
                {
                    _dialogService.ShowWarning("No stored credentials found.");
                    return;
                }

                // Populate fields (but NOT password for security)
                Domain = credentials.Domain;
                Username = credentials.Username;
                CredentialsStoredAt = credentials.StoredAt;

                _dialogService.ShowInformation(
                    $"Loaded credentials for:\nDomain: {credentials.Domain}\nUsername: {credentials.Username}\n\nPassword not shown for security.",
                    "Credentials Loaded");

                _logger.LogInformation("Credentials loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stored credentials");
                _dialogService.ShowError($"Error loading credentials: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task ValidateStoredCredentialsAsync()
        {
            try
            {
                SetBusy(true, "Validating stored credentials...");
                ConnectionStatus = "Validating...";
                IsConnectionSuccessful = false;

                _logger.LogInformation("Validating stored credentials");

                var isValid = await _credentialService.ValidateStoredCredentialsAsync(_adService);

                if (isValid)
                {
                    ConnectionStatus = "✓ Stored credentials are valid!";
                    IsConnectionSuccessful = true;
                    _logger.LogInformation("Stored credentials validated successfully");
                    _dialogService.ShowSuccess("Stored credentials are valid and working!");
                }
                else
                {
                    ConnectionStatus = "✗ Stored credentials are invalid";
                    IsConnectionSuccessful = false;
                    _logger.LogWarning("Stored credentials validation failed");
                    _dialogService.ShowError("Stored credentials are invalid or expired. Please update them.");
                }
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"✗ Validation error: {ex.Message}";
                IsConnectionSuccessful = false;
                _logger.LogError(ex, "Error validating stored credentials");
                _dialogService.ShowError($"Error validating credentials: {ex.Message}");
            }
            finally
            {
                SetBusy(false);
            }
        }

        #endregion
    }
}

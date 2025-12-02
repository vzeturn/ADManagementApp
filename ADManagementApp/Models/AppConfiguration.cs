using System;

namespace ADManagementApp.Models
{
    /// <summary>
    /// SECURE Configuration Management
    /// CRITICAL: Never store passwords in appsettings.json
    /// Use Windows Credential Manager via CredentialService instead
    /// </summary>

    /// <summary>
    /// Active Directory settings - WITHOUT sensitive credentials
    /// Credentials are stored securely in Windows Credential Manager
    /// </summary>
    public class ActiveDirectorySettings
    {
        /// <summary>
        /// Domain name (e.g., "corp.haier.com")
        /// Safe to store in configuration
        /// </summary>
        public string Domain { get; set; } = string.Empty;

        /// <summary>
        /// Default Organizational Unit for new users/groups
        /// Safe to store in configuration
        /// </summary>
        public string DefaultOU { get; set; } = string.Empty;

        /// <summary>
        /// LDAP connection timeout in seconds
        /// </summary>
        public int ConnectionTimeout { get; set; } = 30;

        /// <summary>
        /// Whether to use Windows Credential Manager for secure storage
        /// MUST be true in production
        /// </summary>
        public bool UseSecureCredentialStorage { get; set; } = true;

        // NOTE: No Username or Password properties here!
        // These MUST be retrieved from CredentialService
    }

    /// <summary>
    /// Application behavior settings
    /// </summary>
    public class ApplicationSettings
    {
        public string Theme { get; set; } = "Light";
        public int AutoRefreshInterval { get; set; } = 30;
        public int PageSize { get; set; } = 100;
        public bool EnableAuditLog { get; set; } = true;
        public int MaxSearchResults { get; set; } = 1000;

        /// <summary>
        /// Show security warning if credentials not stored securely
        /// </summary>
        public bool ShowSecurityWarnings { get; set; } = true;

        /// <summary>
        /// Require credential re-authentication after this many hours
        /// 0 = never expire (not recommended)
        /// </summary>
        public int CredentialExpirationHours { get; set; } = 8;
    }

    /// <summary>
    /// Performance optimization settings
    /// </summary>
    public class PerformanceSettings
    {
        public bool EnableCaching { get; set; } = true;
        public int CacheDurationMinutes { get; set; } = 5;
        public int MaxConcurrentOperations { get; set; } = 10;

        /// <summary>
        /// Enable connection pooling for better performance
        /// </summary>
        public bool EnableConnectionPooling { get; set; } = true;
    }

    /// <summary>
    /// Logging configuration
    /// </summary>
    public class LoggingSettings
    {
        public LogLevel LogLevel { get; set; } = new LogLevel();
        public FileLogSettings File { get; set; } = new FileLogSettings();
        public bool LogSensitiveOperations { get; set; } = true;
    }

    public class LogLevel
    {
        public string Default { get; set; } = "Information";
        public string Microsoft { get; set; } = "Warning";
        public string System { get; set; } = "Warning";
        public string Security { get; set; } = "Information";
    }

    public class FileLogSettings
    {
        public string Path { get; set; } = "logs/app-.log";
        public string RollingInterval { get; set; } = "Day";
        public int RetainedFileCountLimit { get; set; } = 7;
        public long FileSizeLimitBytes { get; set; } = 10485760; // 10MB
        public bool RollOnFileSizeLimit { get; set; } = true;
    }

    /// <summary>
    /// Security settings
    /// </summary>
    public class SecuritySettings
    {
        /// <summary>
        /// Require MFA for sensitive operations (delete user, reset password)
        /// </summary>
        public bool RequireMFAForSensitiveOps { get; set; } = false;

        /// <summary>
        /// Lock application after N minutes of inactivity
        /// 0 = never lock
        /// </summary>
        public int AutoLockMinutes { get; set; } = 15;

        /// <summary>
        /// Minimum password complexity score (0-4)
        /// </summary>
        public int MinPasswordComplexity { get; set; } = 3;
    }

    /// <summary>
    /// Root configuration class
    /// </summary>
    public class AppConfiguration
    {
        public ActiveDirectorySettings ActiveDirectory { get; set; } = new();
        public ApplicationSettings Application { get; set; } = new();
        public PerformanceSettings Performance { get; set; } = new();
        public LoggingSettings Logging { get; set; } = new();
        public SecuritySettings Security { get; set; } = new();
    }

    /// <summary>
    /// Credential data retrieved from secure storage
    /// NEVER serialize or log this class!
    /// </summary>
    public class SecureCredentials
    {
        public string Domain { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime StoredAt { get; set; }

        /// <summary>
        /// Check if credentials are expired based on configuration
        /// </summary>
        public bool IsExpired(int expirationHours)
        {
            if (expirationHours == 0)
                return false;
            return DateTime.Now > StoredAt.AddHours(expirationHours);
        }
    }
}

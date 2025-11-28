namespace ADManagementApp.Models
{
    /// <summary>
    /// Strongly-typed configuration models
    /// Provides compile-time safety for application settings
    /// </summary>

    public class ActiveDirectorySettings
    {
        public string Domain { get; set; } = string.Empty;
        public string DefaultOU { get; set; } = string.Empty;
        public int ConnectionTimeout { get; set; } = 30;
        public bool UseSecureCredentialStorage { get; set; } = true;
    }

    public class ApplicationSettings
    {
        public string Theme { get; set; } = "Light";
        public int AutoRefreshInterval { get; set; } = 30;
        public int PageSize { get; set; } = 100;
        public bool EnableAuditLog { get; set; } = true;
        public int MaxSearchResults { get; set; } = 1000;
    }

    public class PerformanceSettings
    {
        public bool EnableCaching { get; set; } = true;
        public int CacheDurationMinutes { get; set; } = 5;
        public int MaxConcurrentOperations { get; set; } = 10;
    }

    public class LoggingSettings
    {
        public LogLevel LogLevel { get; set; } = new LogLevel();
        public FileLogSettings File { get; set; } = new FileLogSettings();
    }

    public class LogLevel
    {
        public string Default { get; set; } = "Information";
        public string Microsoft { get; set; } = "Warning";
        public string System { get; set; } = "Warning";
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
    /// Root configuration class
    /// </summary>
    public class AppConfiguration
    {
        public ActiveDirectorySettings ActiveDirectory { get; set; } = new();
        public ApplicationSettings Application { get; set; } = new();
        public PerformanceSettings Performance { get; set; } = new();
        public LoggingSettings Logging { get; set; } = new();
    }
}
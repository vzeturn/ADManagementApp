using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ADManagementApp.Services
{
    /// <summary>
    /// Audit service for tracking all AD operations
    /// Maintains compliance and security audit trail
    /// </summary>
    public interface IAuditService
    {
        Task LogOperationAsync(string operation, string username, string details, bool success);
        Task LogSecurityEventAsync(string eventType, string username, string details);
    }

    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService> _logger;
        private readonly string _auditLogPath;

        public AuditService(ILogger<AuditService> logger)
        {
            _logger = logger;
            _auditLogPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "audit",
                $"audit-{DateTime.Now:yyyy-MM}.log");

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(_auditLogPath)!);
        }

        public async Task LogOperationAsync(string operation, string username, string details, bool success)
        {
            var auditEntry = new
            {
                Timestamp = DateTime.UtcNow,
                Operation = operation,
                Username = username,
                Details = details,
                Success = success,
                PerformedBy = Environment.UserName,
                Machine = Environment.MachineName
            };

            var json = JsonSerializer.Serialize(auditEntry, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            try
            {
                await File.AppendAllTextAsync(_auditLogPath, json + Environment.NewLine);
                _logger.LogInformation("Audit: {Operation} by {User} - Success: {Success}",
                    operation, username, success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit log");
            }
        }

        public async Task LogSecurityEventAsync(string eventType, string username, string details)
        {
            var securityEntry = new
            {
                Timestamp = DateTime.UtcNow,
                EventType = eventType,
                Username = username,
                Details = details,
                Machine = Environment.MachineName,
                UserAccount = Environment.UserName
            };

            var json = JsonSerializer.Serialize(securityEntry, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            try
            {
                var securityLogPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "audit",
                    $"security-{DateTime.Now:yyyy-MM}.log");

                await File.AppendAllTextAsync(securityLogPath, json + Environment.NewLine);
                _logger.LogWarning("Security Event: {EventType} - User: {Username}", eventType, username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write security log");
            }
        }
    }
}

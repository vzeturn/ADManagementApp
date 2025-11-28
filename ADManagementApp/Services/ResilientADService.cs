using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using ADManagementApp.Models;

namespace ADManagementApp.Services
{
    /// <summary>
    /// Resilient wrapper for AD Service with retry and circuit breaker patterns
    /// Handles transient failures gracefully
    /// </summary>
    public class ResilientADService : IADService
    {
        private readonly IADService _innerService;
        private readonly ILogger<ResilientADService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public ResilientADService(
            IADService innerService,
            ILogger<ResilientADService> logger)
        {
            _innerService = innerService;
            _logger = logger;

            // Configure retry policy with exponential backoff
            _retryPolicy = Policy
                .Handle<System.DirectoryServices.DirectoryServicesCOMException>()
                .Or<System.Runtime.InteropServices.COMException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            "Retry {RetryCount} after {Delay}ms due to: {Exception}",
                            retryCount, timeSpan.TotalMilliseconds, exception.Message);
                    });
        }

        #region Connection Methods

        public async Task<bool> TestConnectionAsync(string domain, string username, string password)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.TestConnectionAsync(domain, username, password));
        }

        public void SetCredentials(string domain, string username, string password, string defaultOU)
        {
            _innerService.SetCredentials(domain, username, password, defaultOU);
        }

        #endregion

        #region Users

        public async Task<List<ADUser>> GetAllUsersAsync(string searchTerm = "")
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.GetAllUsersAsync(searchTerm));
        }

        public async Task<ADUser?> GetUserAsync(string username)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.GetUserAsync(username));
        }

        public async Task<bool> CreateUserAsync(ADUser user, string password)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.CreateUserAsync(user, password));
        }

        public async Task<bool> UpdateUserAsync(ADUser user)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.UpdateUserAsync(user));
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.DeleteUserAsync(username));
        }

        public async Task<bool> EnableUserAsync(string username)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.EnableUserAsync(username));
        }

        public async Task<bool> DisableUserAsync(string username)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.DisableUserAsync(username));
        }

        public async Task<bool> ResetPasswordAsync(string username, string newPassword, bool mustChangePassword)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.ResetPasswordAsync(username, newPassword, mustChangePassword));
        }

        public async Task<bool> UnlockAccountAsync(string username)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.UnlockAccountAsync(username));
        }

        #endregion

        #region Groups

        public async Task<List<ADGroup>> GetAllGroupsAsync(string searchTerm = "")
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.GetAllGroupsAsync(searchTerm));
        }

        public async Task<ADGroup?> GetGroupAsync(string groupname)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.GetGroupAsync(groupname));
        }

        public async Task<bool> CreateGroupAsync(ADGroup group)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.CreateGroupAsync(group));
        }

        public async Task<bool> DeleteGroupAsync(string groupname)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.DeleteGroupAsync(groupname));
        }

        public async Task<bool> AddUserToGroupAsync(string groupname, string username)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.AddUserToGroupAsync(groupname, username));
        }

        public async Task<bool> RemoveUserFromGroupAsync(string groupname, string username)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.RemoveUserFromGroupAsync(groupname, username));
        }

        #endregion

        #region Statistics

        public async Task<DomainStats> GetDomainStatsAsync()
        {
            return await _retryPolicy.ExecuteAsync(async () =>
                await _innerService.GetDomainStatsAsync());
        }

        #endregion
    }
}
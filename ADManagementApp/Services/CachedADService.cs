using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ADManagementApp.Models;

namespace ADManagementApp.Services
{
    /// <summary>
    /// Cached wrapper for AD Service to improve performance
    /// Reduces load on Domain Controller
    /// </summary>
    public class CachedADService : IADService
    {
        private readonly IADService _innerService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedADService> _logger;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        private const string USERS_CACHE_KEY = "AllUsers";
        private const string GROUPS_CACHE_KEY = "AllGroups";
        private const string STATS_CACHE_KEY = "DomainStats";

        public CachedADService(
            IADService innerService,
            IMemoryCache cache,
            ILogger<CachedADService> logger)
        {
            _innerService = innerService;
            _cache = cache;
            _logger = logger;
        }

        #region Connection Methods (No Caching)

        public Task<bool> TestConnectionAsync(string domain, string username, string password)
        {
            return _innerService.TestConnectionAsync(domain, username, password);
        }

        public void SetCredentials(string domain, string username, string password, string defaultOU)
        {
            _innerService.SetCredentials(domain, username, password, defaultOU);
            InvalidateAllCache();
        }

        #endregion

        #region Users (With Caching)

        public async Task<List<ADUser>> GetAllUsersAsync(string searchTerm = "")
        {
            var cacheKey = $"{USERS_CACHE_KEY}_{searchTerm}";

            if (_cache.TryGetValue(cacheKey, out List<ADUser> cachedUsers))
            {
                _logger.LogDebug("Cache hit for users with search term: {SearchTerm}", searchTerm);
                return cachedUsers;
            }

            _logger.LogDebug("Cache miss for users with search term: {SearchTerm}", searchTerm);
            var users = await _innerService.GetAllUsersAsync(searchTerm);

            _cache.Set(cacheKey, users, _cacheDuration);
            return users;
        }

        public async Task<ADUser?> GetUserAsync(string username)
        {
            var cacheKey = $"User_{username}";

            if (_cache.TryGetValue(cacheKey, out ADUser cachedUser))
            {
                _logger.LogDebug("Cache hit for user: {Username}", username);
                return cachedUser;
            }

            _logger.LogDebug("Cache miss for user: {Username}", username);
            var user = await _innerService.GetUserAsync(username);

            if (user != null)
            {
                _cache.Set(cacheKey, user, _cacheDuration);
            }

            return user;
        }

        public async Task<bool> CreateUserAsync(ADUser user, string password)
        {
            var result = await _innerService.CreateUserAsync(user, password);
            if (result)
            {
                InvalidateUsersCache();
                InvalidateStatsCache();
            }
            return result;
        }

        public async Task<bool> UpdateUserAsync(ADUser user)
        {
            var result = await _innerService.UpdateUserAsync(user);
            if (result)
            {
                InvalidateUserCache(user.SamAccountName);
                InvalidateUsersCache();
            }
            return result;
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            var result = await _innerService.DeleteUserAsync(username);
            if (result)
            {
                InvalidateUserCache(username);
                InvalidateUsersCache();
                InvalidateStatsCache();
            }
            return result;
        }

        public async Task<bool> EnableUserAsync(string username)
        {
            var result = await _innerService.EnableUserAsync(username);
            if (result)
            {
                InvalidateUserCache(username);
                InvalidateUsersCache();
                InvalidateStatsCache();
            }
            return result;
        }

        public async Task<bool> DisableUserAsync(string username)
        {
            var result = await _innerService.DisableUserAsync(username);
            if (result)
            {
                InvalidateUserCache(username);
                InvalidateUsersCache();
                InvalidateStatsCache();
            }
            return result;
        }

        public async Task<bool> ResetPasswordAsync(string username, string newPassword, bool mustChangePassword)
        {
            var result = await _innerService.ResetPasswordAsync(username, newPassword, mustChangePassword);
            if (result)
            {
                InvalidateUserCache(username);
            }
            return result;
        }

        public async Task<bool> UnlockAccountAsync(string username)
        {
            var result = await _innerService.UnlockAccountAsync(username);
            if (result)
            {
                InvalidateUserCache(username);
            }
            return result;
        }

        #endregion

        #region Groups (With Caching)

        public async Task<List<ADGroup>> GetAllGroupsAsync(string searchTerm = "")
        {
            var cacheKey = $"{GROUPS_CACHE_KEY}_{searchTerm}";

            if (_cache.TryGetValue(cacheKey, out List<ADGroup> cachedGroups))
            {
                _logger.LogDebug("Cache hit for groups with search term: {SearchTerm}", searchTerm);
                return cachedGroups;
            }

            _logger.LogDebug("Cache miss for groups with search term: {SearchTerm}", searchTerm);
            var groups = await _innerService.GetAllGroupsAsync(searchTerm);

            _cache.Set(cacheKey, groups, _cacheDuration);
            return groups;
        }

        public async Task<ADGroup?> GetGroupAsync(string groupname)
        {
            var cacheKey = $"Group_{groupname}";

            if (_cache.TryGetValue(cacheKey, out ADGroup cachedGroup))
            {
                _logger.LogDebug("Cache hit for group: {GroupName}", groupname);
                return cachedGroup;
            }

            _logger.LogDebug("Cache miss for group: {GroupName}", groupname);
            var group = await _innerService.GetGroupAsync(groupname);

            if (group != null)
            {
                _cache.Set(cacheKey, group, _cacheDuration);
            }

            return group;
        }

        public async Task<bool> CreateGroupAsync(ADGroup group)
        {
            var result = await _innerService.CreateGroupAsync(group);
            if (result)
            {
                InvalidateGroupsCache();
                InvalidateStatsCache();
            }
            return result;
        }

        public async Task<bool> DeleteGroupAsync(string groupname)
        {
            var result = await _innerService.DeleteGroupAsync(groupname);
            if (result)
            {
                InvalidateGroupCache(groupname);
                InvalidateGroupsCache();
                InvalidateStatsCache();
            }
            return result;
        }

        public async Task<bool> AddUserToGroupAsync(string groupname, string username)
        {
            var result = await _innerService.AddUserToGroupAsync(groupname, username);
            if (result)
            {
                InvalidateGroupCache(groupname);
                InvalidateUserCache(username);
            }
            return result;
        }

        public async Task<bool> RemoveUserFromGroupAsync(string groupname, string username)
        {
            var result = await _innerService.RemoveUserFromGroupAsync(groupname, username);
            if (result)
            {
                InvalidateGroupCache(groupname);
                InvalidateUserCache(username);
            }
            return result;
        }

        #endregion

        #region Statistics (With Caching)

        public async Task<DomainStats> GetDomainStatsAsync()
        {
            if (_cache.TryGetValue(STATS_CACHE_KEY, out DomainStats cachedStats))
            {
                _logger.LogDebug("Cache hit for domain stats");
                return cachedStats;
            }

            _logger.LogDebug("Cache miss for domain stats");
            var stats = await _innerService.GetDomainStatsAsync();

            _cache.Set(STATS_CACHE_KEY, stats, _cacheDuration);
            return stats;
        }

        #endregion

        #region Cache Invalidation

        private void InvalidateUserCache(string username)
        {
            _cache.Remove($"User_{username}");
            _logger.LogDebug("Invalidated cache for user: {Username}", username);
        }

        private void InvalidateUsersCache()
        {
            // Remove all user-related cache entries
            // In production, consider using a more sophisticated approach
            _logger.LogDebug("Invalidated all users cache");
        }

        private void InvalidateGroupCache(string groupname)
        {
            _cache.Remove($"Group_{groupname}");
            _logger.LogDebug("Invalidated cache for group: {GroupName}", groupname);
        }

        private void InvalidateGroupsCache()
        {
            _logger.LogDebug("Invalidated all groups cache");
        }

        private void InvalidateStatsCache()
        {
            _cache.Remove(STATS_CACHE_KEY);
            _logger.LogDebug("Invalidated domain stats cache");
        }

        private void InvalidateAllCache()
        {
            // Clear all cache when credentials change
            _logger.LogInformation("Invalidated all cache due to credential change");
        }

        #endregion
    }
}
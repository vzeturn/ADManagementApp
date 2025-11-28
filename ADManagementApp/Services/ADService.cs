using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;
using ADManagementApp.Models;

namespace ADManagementApp.Services
{
    public class ADService : IADService
    {
        private string _domain = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _defaultOU = string.Empty;

        public void SetCredentials(string domain, string username, string password, string defaultOU = "")
        {
            _domain = domain;
            _username = username;
            _password = password;
            _defaultOU = defaultOU;
        }

        public async Task<bool> TestConnectionAsync(string domain, string username, string password)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = new PrincipalContext(ContextType.Domain, domain, username, password))
                    {
                        return context.ConnectedServer != null;
                    }
                }
                catch
                {
                    return false;
                }
            });
        }

        private PrincipalContext GetPrincipalContext()
        {
            if (string.IsNullOrEmpty(_domain))
                throw new InvalidOperationException("Domain credentials not set. Please configure connection first.");

            if (!string.IsNullOrEmpty(_defaultOU))
            {
                return new PrincipalContext(
                    ContextType.Domain,
                    _domain,
                    _defaultOU,
                    _username,
                    _password
                );
            }

            return new PrincipalContext(
                ContextType.Domain,
                _domain,
                _username,
                _password
            );
        }

        #region Users

        public async Task<List<ADUser>> GetAllUsersAsync(string searchTerm = "")
        {
            return await Task.Run(() =>
            {
                var users = new List<ADUser>();
                
                try
                {
                    using (var context = GetPrincipalContext())
                    using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                    {
                        var results = searcher.FindAll()
                            .Cast<UserPrincipal>()
                            .Where(u => string.IsNullOrEmpty(searchTerm) ||
                                       (u.SamAccountName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                       (u.DisplayName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                       (u.EmailAddress?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

                        foreach (var userPrincipal in results)
                        {
                            users.Add(ConvertToADUser(userPrincipal));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error retrieving users: {ex.Message}", ex);
                }

                return users;
            });
        }

        public async Task<ADUser?> GetUserAsync(string username)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        var userPrincipal = UserPrincipal.FindByIdentity(context, username);
                        if (userPrincipal == null)
                            return null;

                        return ConvertToADUser(userPrincipal);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error retrieving user: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> CreateUserAsync(ADUser user, string password)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        var userPrincipal = new UserPrincipal(context)
                        {
                            SamAccountName = user.SamAccountName,
                            DisplayName = user.DisplayName,
                            GivenName = user.GivenName,
                            Surname = user.Surname,
                            EmailAddress = user.EmailAddress,
                            Enabled = user.Enabled ?? true,
                            Description = user.Description,
                            UserPrincipalName = string.IsNullOrEmpty(user.UserPrincipalName) 
                                ? $"{user.SamAccountName}@{_domain}" 
                                : user.UserPrincipalName
                        };

                        userPrincipal.SetPassword(password);
                        userPrincipal.Save();

                        // Set additional properties using DirectoryEntry
                        using (DirectoryEntry de = (DirectoryEntry)userPrincipal.GetUnderlyingObject())
                        {
                            if (!string.IsNullOrEmpty(user.Department))
                                de.Properties["department"].Value = user.Department;
                            if (!string.IsNullOrEmpty(user.Title))
                                de.Properties["title"].Value = user.Title;
                            if (!string.IsNullOrEmpty(user.PhoneNumber))
                                de.Properties["telephoneNumber"].Value = user.PhoneNumber;
                            
                            de.CommitChanges();
                        }

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error creating user: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> UpdateUserAsync(ADUser user)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        var userPrincipal = UserPrincipal.FindByIdentity(context, user.SamAccountName);
                        if (userPrincipal == null)
                            return false;

                        userPrincipal.DisplayName = user.DisplayName;
                        userPrincipal.GivenName = user.GivenName;
                        userPrincipal.Surname = user.Surname;
                        userPrincipal.EmailAddress = user.EmailAddress;
                        userPrincipal.Description = user.Description;
                        if (user.Enabled.HasValue)
                            userPrincipal.Enabled = user.Enabled.Value;

                        userPrincipal.Save();

                        // Update additional properties
                        using (DirectoryEntry de = (DirectoryEntry)userPrincipal.GetUnderlyingObject())
                        {
                            if (!string.IsNullOrEmpty(user.Department))
                                de.Properties["department"].Value = user.Department;
                            if (!string.IsNullOrEmpty(user.Title))
                                de.Properties["title"].Value = user.Title;
                            if (!string.IsNullOrEmpty(user.PhoneNumber))
                                de.Properties["telephoneNumber"].Value = user.PhoneNumber;
                            
                            de.CommitChanges();
                        }

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error updating user: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        var userPrincipal = UserPrincipal.FindByIdentity(context, username);
                        if (userPrincipal == null)
                            return false;

                        userPrincipal.Delete();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error deleting user: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> EnableUserAsync(string username)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        var userPrincipal = UserPrincipal.FindByIdentity(context, username);
                        if (userPrincipal == null)
                            return false;

                        userPrincipal.Enabled = true;
                        userPrincipal.Save();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error enabling user: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> DisableUserAsync(string username)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        var userPrincipal = UserPrincipal.FindByIdentity(context, username);
                        if (userPrincipal == null)
                            return false;

                        userPrincipal.Enabled = false;
                        userPrincipal.Save();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error disabling user: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> ResetPasswordAsync(string username, string newPassword, bool mustChangePassword)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        var userPrincipal = UserPrincipal.FindByIdentity(context, username);
                        if (userPrincipal == null)
                            return false;

                        userPrincipal.SetPassword(newPassword);
                        
                        if (mustChangePassword)
                            userPrincipal.ExpirePasswordNow();
                        
                        userPrincipal.Save();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error resetting password: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> UnlockAccountAsync(string username)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        var userPrincipal = UserPrincipal.FindByIdentity(context, username);
                        if (userPrincipal == null)
                            return false;

                        userPrincipal.UnlockAccount();
                        userPrincipal.Save();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error unlocking account: {ex.Message}", ex);
                }
            });
        }

        #endregion

        #region Groups

        public async Task<List<ADGroup>> GetAllGroupsAsync(string searchTerm = "")
        {
            return await Task.Run(() =>
            {
                var groups = new List<ADGroup>();
                
                try
                {
                    using (var context = GetPrincipalContext())
                    using (var searcher = new PrincipalSearcher(new GroupPrincipal(context)))
                    {
                        var results = searcher.FindAll()
                            .Cast<GroupPrincipal>()
                            .Where(g => string.IsNullOrEmpty(searchTerm) ||
                                       (g.SamAccountName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                       (g.Name?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

                        foreach (var groupPrincipal in results)
                        {
                            groups.Add(ConvertToADGroup(groupPrincipal));
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error retrieving groups: {ex.Message}", ex);
                }

                return groups;
            });
        }

        public async Task<ADGroup?> GetGroupAsync(string groupname)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        var groupPrincipal = GroupPrincipal.FindByIdentity(context, groupname);
                        if (groupPrincipal == null)
                            return null;

                        return ConvertToADGroup(groupPrincipal, includeMembers: true);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error retrieving group: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> CreateGroupAsync(ADGroup group)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        var groupPrincipal = new GroupPrincipal(context)
                        {
                            SamAccountName = group.SamAccountName,
                            Name = group.Name,
                            Description = group.Description
                        };

                        groupPrincipal.Save();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error creating group: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> DeleteGroupAsync(string groupname)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        var groupPrincipal = GroupPrincipal.FindByIdentity(context, groupname);
                        if (groupPrincipal == null)
                            return false;

                        groupPrincipal.Delete();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error deleting group: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> AddUserToGroupAsync(string groupname, string username)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        var groupPrincipal = GroupPrincipal.FindByIdentity(context, groupname);
                        if (groupPrincipal == null)
                            return false;

                        var userPrincipal = UserPrincipal.FindByIdentity(context, username);
                        if (userPrincipal == null)
                            return false;

                        groupPrincipal.Members.Add(userPrincipal);
                        groupPrincipal.Save();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error adding user to group: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> RemoveUserFromGroupAsync(string groupname, string username)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        var groupPrincipal = GroupPrincipal.FindByIdentity(context, groupname);
                        if (groupPrincipal == null)
                            return false;

                        var userPrincipal = UserPrincipal.FindByIdentity(context, username);
                        if (userPrincipal == null)
                            return false;

                        groupPrincipal.Members.Remove(userPrincipal);
                        groupPrincipal.Save();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error removing user from group: {ex.Message}", ex);
                }
            });
        }

        #endregion

        #region Statistics

        public async Task<DomainStats> GetDomainStatsAsync()
        {
            return await Task.Run(() =>
            {
                var stats = new DomainStats { DomainName = _domain };
                
                try
                {
                    using (var context = GetPrincipalContext())
                    {
                        stats.DomainController = context.ConnectedServer ?? "Unknown";

                        using (var userSearcher = new PrincipalSearcher(new UserPrincipal(context)))
                        {
                            var allUsers = userSearcher.FindAll().Cast<UserPrincipal>().ToList();
                            stats.TotalUsers = allUsers.Count;
                            stats.EnabledUsers = allUsers.Count(u => u.Enabled == true);
                            stats.DisabledUsers = allUsers.Count(u => u.Enabled == false);
                        }

                        using (var groupSearcher = new PrincipalSearcher(new GroupPrincipal(context)))
                        {
                            stats.TotalGroups = groupSearcher.FindAll().Count();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error retrieving domain statistics: {ex.Message}", ex);
                }

                return stats;
            });
        }

        #endregion

        #region Helper Methods

        private ADUser ConvertToADUser(UserPrincipal userPrincipal)
        {
            var user = new ADUser
            {
                SamAccountName = userPrincipal.SamAccountName ?? string.Empty,
                DisplayName = userPrincipal.DisplayName ?? string.Empty,
                GivenName = userPrincipal.GivenName ?? string.Empty,
                Surname = userPrincipal.Surname ?? string.Empty,
                EmailAddress = userPrincipal.EmailAddress ?? string.Empty,
                Enabled = userPrincipal.Enabled,
                LastLogon = userPrincipal.LastLogon,
                LastPasswordSet = userPrincipal.LastPasswordSet,
                PasswordNeverExpires = userPrincipal.PasswordNeverExpires,
                UserCannotChangePassword = userPrincipal.UserCannotChangePassword,
                DistinguishedName = userPrincipal.DistinguishedName ?? string.Empty,
                Description = userPrincipal.Description ?? string.Empty,
                UserPrincipalName = userPrincipal.UserPrincipalName ?? string.Empty
            };

            // Get additional properties from DirectoryEntry
            try
            {
                using (DirectoryEntry de = (DirectoryEntry)userPrincipal.GetUnderlyingObject())
                {
                    if (de.Properties.Contains("department") && de.Properties["department"].Value != null)
                        user.Department = de.Properties["department"].Value.ToString() ?? string.Empty;
                    
                    if (de.Properties.Contains("title") && de.Properties["title"].Value != null)
                        user.Title = de.Properties["title"].Value.ToString() ?? string.Empty;
                    
                    if (de.Properties.Contains("telephoneNumber") && de.Properties["telephoneNumber"].Value != null)
                        user.PhoneNumber = de.Properties["telephoneNumber"].Value.ToString() ?? string.Empty;
                }
            }
            catch { }

            // Get groups
            try
            {
                user.Groups = userPrincipal.GetGroups()
                    .Select(g => g.Name)
                    .Where(name => name != null)
                    .Cast<string>()
                    .ToList();
            }
            catch { }

            return user;
        }

        private ADGroup ConvertToADGroup(GroupPrincipal groupPrincipal, bool includeMembers = false)
        {
            var group = new ADGroup
            {
                SamAccountName = groupPrincipal.SamAccountName ?? string.Empty,
                Name = groupPrincipal.Name ?? string.Empty,
                Description = groupPrincipal.Description ?? string.Empty,
                DistinguishedName = groupPrincipal.DistinguishedName ?? string.Empty
            };

            try
            {
                var members = groupPrincipal.GetMembers().ToList();
                group.MemberCount = members.Count;

                if (includeMembers)
                {
                    group.Members = members.Select(m => new ADGroupMember
                    {
                        SamAccountName = m.SamAccountName ?? string.Empty,
                        DisplayName = m.DisplayName ?? m.Name ?? string.Empty,
                        Type = m.StructuralObjectClass ?? "unknown"
                    }).ToList();
                }
            }
            catch
            {
                group.MemberCount = 0;
            }

            return group;
        }

        #endregion
    }
}
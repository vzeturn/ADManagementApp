using System;
using System.Collections.Generic;

namespace ADManagementApp.Models
{
    public class ADUser
    {
        public string SamAccountName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public bool? Enabled { get; set; }
        public DateTime? LastLogon { get; set; }
        public DateTime? LastPasswordSet { get; set; }
        public bool PasswordNeverExpires { get; set; }
        public bool UserCannotChangePassword { get; set; }
        public string DistinguishedName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserPrincipalName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Manager { get; set; } = string.Empty;
        public List<string> Groups { get; set; } = new List<string>();
        
        // Display properties
        public string StatusText => Enabled == true ? "Enabled" : "Disabled";
        public string LastLogonText => LastLogon?.ToString("dd/MM/yyyy HH:mm") ?? "Never";
        public string GroupsText => Groups.Count > 0 ? string.Join(", ", Groups) : "No groups";
    }
}
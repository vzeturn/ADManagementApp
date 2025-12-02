using System;
using System.Collections.Generic;

namespace ADManagementApp.Models
{
    /// <summary>
    /// Represents an Active Directory user with all related properties.
    /// This model is used throughout the application for displaying and managing user data.
    /// </summary>
    public class ADUser
    {
        /// <summary>
        /// Gets or sets the SAM account name (Windows logon name).
        /// </summary>
        public string SamAccountName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's given (first) name.
        /// </summary>
        public string GivenName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's surname (last) name.
        /// </summary>
        public string Surname { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's email address.
        /// </summary>
        public string EmailAddress { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the user account is enabled.
        /// </summary>
        public bool? Enabled { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the user's last logon.
        /// </summary>
        public DateTime? LastLogon { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the user's password was last changed.
        /// </summary>
        public DateTime? LastPasswordSet { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user's password never expires.
        /// </summary>
        public bool PasswordNeverExpires { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can change their own password.
        /// </summary>
        public bool UserCannotChangePassword { get; set; }

        /// <summary>
        /// Gets or sets the distinguished name of the user in Active Directory.
        /// </summary>
        public string DistinguishedName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's principal name.
        /// </summary>
        public string UserPrincipalName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's department.
        /// </summary>
        public string Department { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's job title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the user's phone number.
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the distinguished name of the user's manager.
        /// </summary>
        public string Manager { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of groups the user belongs to.
        /// </summary>
        public List<string> Groups { get; set; } = new List<string>();

        /// <summary>
        /// Gets the status text ("Enabled" or "Disabled") based on the Enabled property.
        /// </summary>
        public string StatusText => Enabled == true ? "Enabled" : "Disabled";

        /// <summary>
        /// Gets the formatted last logon date string (dd/MM/yyyy HH:mm or "Never").
        /// </summary>
        public string LastLogonText => LastLogon?.ToString("dd/MM/yyyy HH:mm") ?? "Never";

        /// <summary>
        /// Gets the formatted list of groups comma-separated or "No groups".
        /// </summary>
        public string GroupsText => Groups.Count > 0 ? string.Join(", ", Groups) : "No groups";
    }
}

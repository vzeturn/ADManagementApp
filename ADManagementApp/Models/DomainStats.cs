using System;

namespace ADManagementApp.Models
{
    /// <summary>
    /// Represents statistics about an Active Directory domain.
    /// </summary>
    public class DomainStats
    {
        /// <summary>
        /// Gets or sets the total number of users in the domain.
        /// </summary>
        public int TotalUsers { get; set; }

        /// <summary>
        /// Gets or sets the number of enabled users in the domain.
        /// </summary>
        public int EnabledUsers { get; set; }

        /// <summary>
        /// Gets or sets the number of disabled users in the domain.
        /// </summary>
        public int DisabledUsers { get; set; }

        /// <summary>
        /// Gets or sets the total number of groups in the domain.
        /// </summary>
        public int TotalGroups { get; set; }

        /// <summary>
        /// Gets or sets the name of the domain.
        /// </summary>
        public string DomainName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the domain controller.
        /// </summary>
        public string DomainController { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when these statistics were last updated.
        /// </summary>
        public DateTime? LastUpdated { get; set; }

        /// <summary>
        /// Gets the percentage of enabled users in the domain (0-100).
        /// </summary>
        public double EnabledPercentage => TotalUsers > 0 ? (double)EnabledUsers / TotalUsers * 100 : 0;

        /// <summary>
        /// Gets the percentage of disabled users in the domain (0-100).
        /// </summary>
        public double DisabledPercentage => TotalUsers > 0 ? (double)DisabledUsers / TotalUsers * 100 : 0;
    }
}

using System;

namespace ADManagementApp.Models
{
    public class DomainStats
    {
        public int TotalUsers { get; set; }
        public int EnabledUsers { get; set; }
        public int DisabledUsers { get; set; }
        public int TotalGroups { get; set; }
        public string DomainName { get; set; } = string.Empty;
        public string DomainController { get; set; } = string.Empty;
        public DateTime? LastUpdated { get; set; }

        public double EnabledPercentage => TotalUsers > 0 ? (double)EnabledUsers / TotalUsers * 100 : 0;
        public double DisabledPercentage => TotalUsers > 0 ? (double)DisabledUsers / TotalUsers * 100 : 0;
    }
}
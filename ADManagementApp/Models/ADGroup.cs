using System.Collections.Generic;

namespace ADManagementApp.Models
{
    public class ADGroup
    {
        public string SamAccountName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DistinguishedName { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public List<ADGroupMember> Members { get; set; } = new List<ADGroupMember>();
        
        public string MemberCountText => $"{MemberCount} member(s)";
    }

    public class ADGroupMember
    {
        public string SamAccountName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string TypeIcon => Type == "user" ? "Account" : "AccountGroup";
    }
}
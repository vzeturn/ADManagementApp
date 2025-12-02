using System.Collections.Generic;

namespace ADManagementApp.Models
{
    /// <summary>
    /// Represents an Active Directory group with all related properties.
    /// </summary>
    public class ADGroup
    {
        /// <summary>
        /// Gets or sets the SAM account name of the group.
        /// </summary>
        public string SamAccountName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name of the group.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the group.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the distinguished name of the group in Active Directory.
        /// </summary>
        public string DistinguishedName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of members in this group.
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// Gets or sets the list of members in this group.
        /// </summary>
        public List<ADGroupMember> Members { get; set; } = new List<ADGroupMember>();

        /// <summary>
        /// Gets the formatted member count text (e.g., "5 member(s)").
        /// </summary>
        public string MemberCountText => $"{MemberCount} member(s)";
    }

    /// <summary>
    /// Represents a member of an Active Directory group.
    /// </summary>
    public class ADGroupMember
    {
        /// <summary>
        /// Gets or sets the SAM account name of the group member.
        /// </summary>
        public string SamAccountName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the display name of the group member.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of the member ("user" or "group").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets the appropriate icon for this member type ("Account" for users, "AccountGroup" for groups).
        /// </summary>
        public string TypeIcon => Type == "user" ? "Account" : "AccountGroup";
    }
}

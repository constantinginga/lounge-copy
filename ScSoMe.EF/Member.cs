using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class Member
    {
        public Member()
        {
            MemberConnectionConnecteds = new HashSet<MemberConnection>();
            MemberConnectionMembers = new HashSet<MemberConnection>();
            Participants = new HashSet<Participant>();
        }

        public int MemberId { get; set; }
        public DateTimeOffset CreatedDt { get; set; }
        public DateTimeOffset UpdatedDt { get; set; }
        public string Name { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Json { get; set; }

        public virtual ActivitySection ActivitySection { get; set; } = null!;
        public virtual ContactsSection ContactsSection { get; set; } = null!;
        public virtual DescriptionSection DescriptionSection { get; set; } = null!;
        public virtual ExternalLinksSection ExternalLinksSection { get; set; } = null!;
        public virtual ServicesSection ServicesSection { get; set; } = null!;
        public virtual WorkExperienceSection WorkExperienceSection { get; set; } = null!;
        public virtual ICollection<MemberConnection> MemberConnectionConnecteds { get; set; }
        public virtual ICollection<MemberConnection> MemberConnectionMembers { get; set; }
        public virtual ICollection<Participant> Participants { get; set; }
    }
}

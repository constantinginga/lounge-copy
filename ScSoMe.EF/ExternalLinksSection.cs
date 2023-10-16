using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class ExternalLinksSection
    {
        public ExternalLinksSection()
        {
            ExternalLinks = new HashSet<ExternalLink>();
        }

        public int MemberId { get; set; }
        public bool? PrivacySetting { get; set; }

        public virtual Member Member { get; set; } = null!;
        public virtual ICollection<ExternalLink> ExternalLinks { get; set; }
    }
}

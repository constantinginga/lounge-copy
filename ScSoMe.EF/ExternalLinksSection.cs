using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class ExternalLinksSection
    {
        public int MemberId { get; set; }
        public string? Content { get; set; }
        public bool? PrivacySetting { get; set; }

        public virtual Member Member { get; set; } = null!;
    }
}

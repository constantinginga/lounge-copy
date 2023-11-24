using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class ExternalLink
    {
        public int ExternalLinkId { get; set; }
        public int? MemberId { get; set; }
        public string? Title { get; set; }
        public string? Url { get; set; }

        public virtual ExternalLinksSection? Member { get; set; }
    }
}

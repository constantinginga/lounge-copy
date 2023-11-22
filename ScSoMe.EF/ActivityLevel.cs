using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class ActivityLevel
    {
        public int ActivityLevelId { get; set; }
        public int? MemberId { get; set; }
        public string? GroupName { get; set; }
        public long? Posts { get; set; }
        public long? Comments { get; set; }
        public long? Likes { get; set; }

        public virtual ActivitySection? Member { get; set; }
    }
}

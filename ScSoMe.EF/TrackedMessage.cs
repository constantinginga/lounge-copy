using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class TrackedMessage
    {
        public int MemberId { get; set; }
        public long PostId { get; set; }
        public bool? ManualTrack { get; set; }
        public bool? Liked { get; set; }
        public bool? Commented { get; set; }
        public bool? IsPostCreator { get; set; }
        public bool? Mentioned { get; set; }
        public DateTime? InsertedDt { get; set; }
        public DateTime? UpdatedDt { get; set; }
    }
}

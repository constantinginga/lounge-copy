using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScSoMe.ApiDtos
{
    public class TrackedMessage
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

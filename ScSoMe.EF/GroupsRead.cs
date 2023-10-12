using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class GroupsRead
    {
        public int MemberId { get; set; }
        public int GroupId { get; set; }
        public DateTime LastReadDt { get; set; }
        public bool NotifyOnNew { get; set; }
    }
}

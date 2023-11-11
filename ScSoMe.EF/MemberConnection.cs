using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class MemberConnection
    {
        public int ConnectionId { get; set; }
        public int? MemberId { get; set; }
        public int? ConnectedId { get; set; }

        public virtual Member? Connected { get; set; }
        public virtual Member? Member { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class BlockedMember
    {
        public int MemberId { get; set; }
        public int BlockedMemberId { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class MemberEmailSubscription
    {
        public int MemberId { get; set; }
        public bool NewPosts { get; set; }
        public bool Comments { get; set; }
        public bool Mentions { get; set; }
    }
}

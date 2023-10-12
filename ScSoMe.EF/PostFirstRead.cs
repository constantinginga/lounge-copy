using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class PostFirstRead
    {
        public long RootCommentId { get; set; }
        public int MemberId { get; set; }
        public DateTime FirstDt { get; set; }
    }
}

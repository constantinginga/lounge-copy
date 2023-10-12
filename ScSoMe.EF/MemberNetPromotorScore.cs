using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class MemberNetPromotorScore
    {
        public int MemberId { get; set; }
        public DateTime ReportDate { get; set; }
        public byte Nps { get; set; }
        public string? Sugestion { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class MemberToken
    {
        public string Token { get; set; } = null!;
        public int MemberId { get; set; }
        public string DeviceId { get; set; } = null!;
        public DateTime CreatedDt { get; set; }
        public DateTime? UpdatedDt { get; set; }
    }
}

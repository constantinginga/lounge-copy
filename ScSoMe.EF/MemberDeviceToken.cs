using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class MemberDeviceToken
    {
        public int MemberId { get; set; }
        public string DeviceToken { get; set; } = null!;
        public bool? LoggedOut { get; set; }
    }
}

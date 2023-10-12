using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class Dcnotification
    {
        public int NotificationId { get; set; }
        public int MessageId { get; set; }
        public DateTime CreatedDt { get; set; }
        public string NotificationMessage { get; set; } = null!;
        public DateTime EmailedDt { get; set; }
        public string SubscribersJson { get; set; } = null!;

        public virtual Message Message { get; set; } = null!;
    }
}

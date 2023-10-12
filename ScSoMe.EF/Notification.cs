using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class Notification
    {
        public int NotificationId { get; set; }
        public string? NotificationMessage { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? SubscribersJson { get; set; }
        public long? PostId { get; set; }
        public DateTime? EmailedDt { get; set; }
        public int? GroupId { get; set; }
        public long? CommentId { get; set; }
    }
}

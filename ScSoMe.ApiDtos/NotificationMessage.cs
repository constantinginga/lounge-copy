using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScSoMe.ApiDtos
{
    public class NotificationMessage
    {
        public int NotificationId { get; set; }
        public string? Message { get; set; }
        public long? PostId { get; set; }
        public bool IsRead { get; set; }

        public int? GroupId { get; set; }
        public long? CommentId { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}

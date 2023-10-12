using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class Message
    {
        public int MessageId { get; set; }
        public int ChatId { get; set; }
        public int SenderMemberId { get; set; }
        public string SenderName { get; set; } = null!;
        public string IsRead { get; set; } = null!;
        public DateTime CreatedDt { get; set; }
        public string Text { get; set; } = null!;
        public string? MediaUrl { get; set; }

        public virtual Chat Chat { get; set; } = null!;
    }
}

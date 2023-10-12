using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class Chat
    {
        public Chat()
        {
            Messages = new HashSet<Message>();
            Participants = new HashSet<Participant>();
        }

        public int ChatId { get; set; }
        public DateTime CreatedDt { get; set; }
        public string Chatgroupname { get; set; } = null!;
        public string Displayname { get; set; } = null!;
        public string? Newdisplayname { get; set; }

        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<Participant> Participants { get; set; }
    }
}

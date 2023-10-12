using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScSoMe.RazorLibrary.Pages.Helpers
{
    public class MessageSignalR
    {
        public string message { get; set; }
        public int senderId { get; set; }
        public string senderName { get; set; }
        public string avatar { get; set; }
        public string isRead { get; set; }
        public DateTime sentDate { get; set; }
    }
}

using API;
using Blazorise;
using System;
namespace ScSoMe.RazorLibrary.Pages.Helpers
{
	public class ChatMember: IEquatable<ChatMember>
    {
        public ChatMember()
        {
            this.groupUserIsIn = "";
            this.memberName = "";
            this.avatar = "";
            this.groupDisplayName = "";
            this.latestGroupMessageInfo = new MessageHistory();
            this.newDisplayName = "";
            this.unreadMessages = 0;

        }

        public ChatMember(string groupUserIsIn, string memberName, string avatar, string groupDisplayName, MessageHistory latestGroupMessageInfo, string newDisplayName, int unreadMessages)
        {
            this.groupUserIsIn = groupUserIsIn;
            this.memberName = memberName;
            this.avatar = avatar;
            this.groupDisplayName = groupDisplayName;
            this.latestGroupMessageInfo = latestGroupMessageInfo;
            this.newDisplayName = newDisplayName;
            this.unreadMessages = unreadMessages;
        }

        public string groupUserIsIn { get; set; }
		public string memberName { get; set; }
		public string avatar { get; set; }
        public string groupDisplayName { get; set; }
        public string newDisplayName { get; set; }
        public MessageHistory latestGroupMessageInfo { get; set; }
        public int unreadMessages { get; set; }





        public bool Equals(ChatMember other)
        {
            return this.memberName == other.memberName;
        }
    }
}


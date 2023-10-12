namespace ScSoMe.API.Controllers.DirectMessaging
{
	public class MessageHistory
	{
		public int chatId { get; set; }
		public string message { get; set; }
		public int senderId { get; set; }
		public string senderName { get; set; }
		public string isRead { get; set; }
		public DateTime sentDate { get; set; }

		public MessageHistory()
		{
			this.chatId = default;
			this.message = "";
			this.senderId = default;
			this.senderName = "";
			this.isRead = "false";
			this.sentDate = DateTime.Now;
		}

		public MessageHistory(int chatId, string message, int senderId, string senderName, string isRead, DateTime sentDate)
		{
			this.chatId = chatId;
			this.message = message;
			this.senderId = senderId;
			this.senderName = senderName;
			this.isRead = isRead;
			this.sentDate = sentDate;
		}
	}
}

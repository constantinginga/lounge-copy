using Microsoft.EntityFrameworkCore;
using ScSoMe.API.Controllers.DirectMessaging;
using ScSoMe.EF;
using EF6 = ScSoMe.EF;
namespace ScSoMe.API.Services
{
    public class DCMessageService
    {
        //ATTRIBUTES
        private readonly ILogger<MessageController> _logger;
        private readonly EF6.ScSoMeContext db;

        public DCMessageService(ILogger<MessageController> logger)
        {
            _logger = logger;
            this.db = new EF6.ScSoMeContext();
        }

        public async Task<DcHistory> CreateOnetoOne(string groupName, int senderId, int receiverId, string displayName)
        {
           
                var now = DateTime.Now;

                var dbChat = new EF.Chat()
                {
                    CreatedDt = now,
                    Chatgroupname = groupName,
                    Displayname = displayName,
                    Newdisplayname = null
                };
                db.Chats.Add(dbChat);
                db.SaveChanges();

                var dbParticipant = new EF.Participant()
                {
                    ChatId = dbChat.ChatId,
                    MemberId = senderId
                };
                db.Participants.Add(dbParticipant);

                var dbParticipant2 = new EF.Participant()
                {
                    ChatId = dbChat.ChatId,
                    MemberId = receiverId
                };
                db.Participants.Add(dbParticipant2);
                DcHistory returnObj = new DcHistory(groupName, displayName, null);
                db.SaveChanges();

                return returnObj;
        }

        public async Task<DcHistory> CheckIfExisting(int senderId, int receiverId, string groupName, string displayName)
        {
            string[] split = groupName.Split(',');
            string groupName2 = split[1] + "," + split[0];
            DcHistory displayReturn = new DcHistory(groupName, displayName, null);

            var chat = db.Chats.FirstOrDefault(x => x.Chatgroupname == groupName || x.Chatgroupname == groupName2);
            if (chat != null)
            {
                var parti1 = db.Participants.FirstOrDefault(x => x.ChatId == chat.ChatId && x.MemberId == senderId);
                var parti2 = db.Participants.FirstOrDefault(x => x.ChatId == chat.ChatId && x.MemberId == receiverId);

                if (parti1 != null && parti2 != null)
                {
                    displayReturn = new DcHistory(chat.Chatgroupname, chat.Displayname, chat.Newdisplayname);
                }
            }
            else
            {
                displayReturn = await CreateOnetoOne(groupName, senderId, receiverId, displayName);
            }

            return displayReturn;
        }

        public async Task<bool> CheckIfDeleted(string groupName)
        {

            string[] split = groupName.Split(',');
            string groupName2 = split[1] + "," + split[0];
            bool isDeleted = false;
            var chat = db.Chats.FirstOrDefault(x => x.Chatgroupname == groupName || x.Chatgroupname == groupName2);
            if (chat != null)
            {
                isDeleted = false;
            }
            else
            {
                isDeleted = true;
            }

            return isDeleted;
        }

        public async void MarkMessagesAsRead(string groupName, int currentUserId)
        {

            string[] split = groupName.Split(',');
            string groupName2 = split[1] + "," + split[0];

            var dbChat = db.Chats.FirstOrDefault(x => x.Chatgroupname == groupName || x.Chatgroupname == groupName2);
            var dbParticipant = db.Participants.FirstOrDefault(v => v.ChatId == dbChat.ChatId && v.MemberId != currentUserId);
            var dbMessage = db.Messages.Where(c => c.ChatId == dbChat.ChatId && c.SenderMemberId == dbParticipant.MemberId);
            foreach (var message in dbMessage)
            {
                if (message != null)
                {
                   message.IsRead = "true";
                }
            }
            db.SaveChanges();
        }

        public IEnumerable<MessageHistory> GetChatMessageHistory(string groupName)
        {
            string[] split = groupName.Split(',');
            string groupName2 = split[1] + "," + split[0];

            var dbChat = db.Chats.FirstOrDefault(x => x.Chatgroupname == groupName || x.Chatgroupname == groupName2);
            var dbMessage = db.Messages.Where(x => x.ChatId == dbChat.ChatId).OrderByDescending(c => c.MessageId);

            var result = dbMessage.Select(x => new MessageHistory()
            {
                chatId = x.ChatId,
                message = x.Text,
                senderId = x.SenderMemberId,
                senderName = x.SenderName,
                isRead = x.IsRead,
                sentDate = x.CreatedDt
            });

            return result;
        }

        public async void SaveMessage(string groupName, string text, int senderId, string senderName)
        {
            string[] split = groupName.Split(',');
            string groupName2 = split[1] + "," + split[0];
            var now = DateTime.Now;
            var chat = db.Chats.FirstOrDefault(x => x.Chatgroupname == groupName || x.Chatgroupname == groupName2);

            var dbMessage = new EF.Message()
            {
               ChatId = chat.ChatId,
               SenderMemberId = senderId,
               SenderName = senderName,
               IsRead = "false",
               CreatedDt = now,
               Text = text,
               MediaUrl = null
            };
            db.Messages.Add(dbMessage);
            db.SaveChanges();
        }

    }
}

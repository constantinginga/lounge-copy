using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using ScSoMe.API.Services;
using ScSoMe.ApiDtos;
using ScSoMe.EF;
using System.Linq;
using EF6 = ScSoMe.EF;

namespace ScSoMe.API.Controllers.DirectMessaging
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        //ATTRIBUTES
        private readonly ILogger<MessageController> _logger;
        private readonly EF6.ScSoMeContext db;
        private readonly TrackingMessageService trackingService;
        private HttpClient client;  
        private DCMessageService messageService;


        public MessageController(ILogger<MessageController> logger)
        {
            _logger = logger;
            db = new EF6.ScSoMeContext();
            trackingService = new TrackingMessageService();
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Basic d290aWc5ODkwN0BkdWZlZWQuY29tOlcwUTl6eE5peE9hRlhUN3VSRG9X");
            messageService = new(logger);
        }

        //Moved to MessageService

        //[HttpPost("CreateOnetoOne")]
        //[ProducesResponseType(500)]
        //public async Task<DcHistory> CreateOnetoOne(string groupName, int senderId,int receiverId, string displayName)
        //{
        //    try
        //    {
        //        var result = await messageService.CreateOnetoOne(groupName, senderId, receiverId, displayName);
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.StatusCode = 500;
        //        return null;
        //    }
        //}

        //[HttpPost("CreateManyToMany")]
        //[ProducesResponseType(500)]
        //public async Task<DcHistory> CreateManyToMany(string groupName, int senderId, List<MinimalMemberInfo> allChatUsers, string displayName)
        //{
        //    try
        //    {
        //        var now = DateTime.Now;

        //        var dbChat = new EF.Chat()
        //        {
        //            CreatedDt = now,
        //            Chatgroupname = groupName,
        //            Displayname = displayName,
        //            Newdisplayname = null
        //        };
        //        db.Chats.Add(dbChat);
        //        db.SaveChanges();

        //        var dbParticipant = new EF.Participant()
        //        {
        //            ChatId = dbChat.ChatId,
        //            MemberId = senderId
        //        };
        //        db.Participants.Add(dbParticipant);

        //        foreach(var participants in allChatUsers)
        //        {
        //            var dbParticipant2 = new EF.Participant()
        //            {
        //                ChatId = dbChat.ChatId,
        //                MemberId = participants.Id
        //            };
        //            db.Participants.Add(dbParticipant2);
        //        }
                
        //        DcHistory returnObj = new DcHistory(groupName, displayName, null);
        //        db.SaveChanges();


        //        return returnObj;
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.StatusCode = 500;
        //        return null;
        //    }
        //}

        [HttpGet("CheckChatExist")]
		[ProducesResponseType(200)]
		[ProducesResponseType(500)]
        [Produces("text/json")]
        public async Task<DcHistory> CheckIfExisting(int senderId, int receiverId, string groupName, string displayName)
        {
            var result = await messageService.CheckIfExisting(senderId, receiverId, groupName, displayName);
            return result;
        }

        //[HttpGet("CheckMultiChatExist")]
        //[ProducesResponseType(200)]
        //[ProducesResponseType(500)]
        //[Produces("text/json")]
        //public async Task<DcHistory> CheckMultiChatExist(int senderId, List<MinimalMemberInfo> allChatUsers, string groupName, string displayName)
        //{
        //    DcHistory displayReturn = new DcHistory(groupName, displayName, null);
        //        var chat = db.Chats.FirstOrDefault(x => x.Chatgroupname == groupName);
        //        if (chat != null)
        //        {

        //                displayReturn = new DcHistory(chat.Chatgroupname, chat.Displayname, chat.Newdisplayname);

        //        }
        //        else
        //        {
        //            displayReturn = await CreateManyToMany(groupName, senderId, allChatUsers, displayName);
        //        }

        //        return displayReturn;

        //}

        [HttpPost("SaveMessage")]
		[ProducesResponseType(201)]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
		public async void SaveMessage(string groupName, string text, int senderId, string senderName)
		{
			try
			{
				messageService.SaveMessage(groupName,text,senderId, senderName);
                Response.StatusCode = 200;
            }
			catch (Exception ex)
			{
				Response.StatusCode = 500;
                _logger.LogError(ex,"Error while trying to save a message");
			}
		}

        [HttpGet("GetChatMessageHistory")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [Produces("text/json")]
        public IEnumerable<MessageHistory> GetChatMessageHistory(string groupName)
        {
            var result = messageService.GetChatMessageHistory(groupName);
            return result;
        }

        [HttpGet("GetLastChatMessage")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [Produces("text/json")]
        public MessageHistory GetLastChatMessage(string groupName)
        {
                MessageHistory tmp = new MessageHistory();

                string[] split = groupName.Split(',');
                string groupName2 = split[1] + "," + split[0];

                var dbChat = db.Chats.FirstOrDefault(x => x.Chatgroupname == groupName || x.Chatgroupname == groupName2);
            if(dbChat != null)
            {
                var dbMessage = db.Messages.OrderByDescending(c => c.MessageId).FirstOrDefault(x => x.ChatId == dbChat.ChatId);
                if(dbMessage == null)
                {
                    tmp = new MessageHistory(dbChat.ChatId,"",default,"","false", DateTime.Now);
                    return tmp;
                }
                else
                {
                    tmp = new MessageHistory(dbMessage.ChatId, dbMessage.Text, dbMessage.SenderMemberId, dbMessage.SenderName, dbMessage.IsRead, dbMessage.CreatedDt);

                    return tmp;
                }
            }
            else
            {
                return new MessageHistory();
                //throw new  Exception("No group with the name " + groupName + " exists in the system");
            }
                   
        }

        [HttpGet("GetAllChatsByUser")]
        [ProducesResponseType(200)]
		[ProducesResponseType(500)]
		[Produces("text/json")]
		public List<DcHistory> GetAllChatsByUser(int currentUserId)
		{
            var dbParticipant = db.Participants.Where(x => x.MemberId == currentUserId);
            if(dbParticipant != null)
            {
                List<string> tmp = new List<string>();
                foreach (var participant in dbParticipant)
                {
                    tmp.Add(participant.ChatId.ToString());
                }

                List<DcHistory> result = new List<DcHistory>();
                for (int i = 0; i < tmp.Count; i++)
                {
                    var dbChat = db.Chats.FirstOrDefault(x => x.ChatId.ToString() == tmp[i]);
                    DcHistory d = new DcHistory(dbChat.Chatgroupname, dbChat.Displayname, dbChat.Newdisplayname);

                    result.Add(d);
                }

                return result;	 
            }
            else
            {
                throw new Exception("User does not exists with an ID: " + currentUserId);
            }
        }

        [HttpPut("MarkMessagesAsRead")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async void MarkMessagesAsRead(string groupName,int currentUserId)
        {
            try
            {
                messageService.MarkMessagesAsRead(groupName,currentUserId);
                Response.StatusCode = 200;
                
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
            }
        }

        [HttpGet("GetUnreadMessageCount")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<int> GetUnreadCount(string groupName, int currentUserId)
        {
            int count = 0;
            string[] split = groupName.Split(',');
            string groupName2 = split[1] + "," + split[0];
            string read;
            var dbChat = db.Chats.FirstOrDefault(x => x.Chatgroupname == groupName || x.Chatgroupname == groupName2);
            if(dbChat != null)
            {
                var dbParticipant = db.Participants.FirstOrDefault(v => v.ChatId == dbChat.ChatId && v.MemberId != currentUserId);
                var dbMessage = db.Messages.Where(c => c.ChatId == dbChat.ChatId && c.SenderMemberId == dbParticipant.MemberId);
                foreach (var db in dbMessage)
                {
                    read = db.IsRead;
                    if (read == "false")
                    {
                        count++;
                    }
                }
                return count;
            }
            return count;
        }

        [HttpGet("GetTotalUnreadMessageCount")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<int> GetTotalUnreadCount(int currentUserId)
        {

                int unreadCount = 0;
                List<DcHistory> userChats = new List<DcHistory>();
                userChats = GetAllChatsByUser(currentUserId);
                foreach(var chat in userChats)
                {
                    unreadCount += await GetUnreadCount(chat.groupName, currentUserId);
;                }
                return unreadCount;
        }

        [HttpDelete("DeleteChat")]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async void DeleteChat(string groupName)
        {
                string[] split = groupName.Split(',');
                string groupName2 = split[1] + "," + split[0];

                var dbChat = db.Chats.FirstOrDefault(x => x.Chatgroupname == groupName || x.Chatgroupname == groupName2);
            if(dbChat != null)
            {
                var dbMessages = db.Messages.Where(c => c.ChatId == dbChat.ChatId);
                foreach(var i in dbMessages)
                {
                    db.Remove(i);
                }

                var dbPart = db.Participants.Where(c => c.ChatId == dbChat.ChatId);
                foreach(var j in dbPart)
                {
                    db.Remove(j);
                }
                db.Remove(dbChat);
            }

                Response.StatusCode = 204;
                db.SaveChanges();
        }

        [HttpDelete("DeleteSingleMessage")]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async void DeleteSingleMessage(string groupName, string message, int senderId, DateTime date)
        {
            string[] split = groupName.Split(',');
            string groupName2 = split[1] + "," + split[0];

            var dbChat = db.Chats.FirstOrDefault(x => x.Chatgroupname == groupName || x.Chatgroupname == groupName2);

            var dbMessages = db.Messages.Where(c => c.ChatId == dbChat.ChatId && c.Text == message && c.SenderMemberId == senderId && c.CreatedDt.Second == date.Second);
            if(dbMessages != null)
            {
                foreach (var i in dbMessages)
                {
                    db.Remove(i);
                }
            }



            Response.StatusCode = 204;
            db.SaveChanges();
        }

        [HttpPut("ChangeChatDisplayName")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async void ChangeChatDisplayName(string groupName, string newDisplayName)
        {
            try
            {
                string[] split = groupName.Split(',');
                string groupName2 = split[1] + "," + split[0];

                var dbChat = db.Chats.FirstOrDefault(x => x.Chatgroupname == groupName || x.Chatgroupname == groupName2);
                if(dbChat != null)
                {
                    dbChat.Newdisplayname = newDisplayName;
                }
                Response.StatusCode = 200;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
            }
        }

        [HttpGet("CheckIfDeleted")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [Produces("text/json")]
        public async Task<bool> CheckIfDeleted(string groupName)
        {
            var result = await messageService.CheckIfDeleted(groupName);
            return result;
        }
    }
}
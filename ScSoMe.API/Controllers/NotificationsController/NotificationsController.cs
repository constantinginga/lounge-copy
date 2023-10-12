using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScSoMe.API.Services;
using ScSoMe.ApiDtos;
using ScSoMe.EF;

namespace ScSoMe.API.Controllers.NotificationsController
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly ILogger<NotificationsController> _logger;
        private readonly NotificationsService notificationsService;

        public NotificationsController(ILogger<NotificationsController> logger)
        {
            _logger = logger;
            notificationsService = new NotificationsService();
        }

        [HttpGet("GetMemberNotifications")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public List<NotificationMessage> GetMemberNotifications()
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            int memberId = apiSession.MyMemberId.Value;

            return notificationsService.GetNotificationsForMember(memberId);
        }


        [HttpGet("GetMemberNotificationsCount")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public int GetMemberNotificationsCount(int memberId)
        {
            try
            {
                var result = notificationsService.GetNotificationsForMember(memberId);
                return result.Count(x => !x.IsRead);
            }
            catch (Exception)
            {
                return 0;
            }
        }


        [HttpPost("RemoveSubscriberNotification")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task RemoveSubscriberNotification(int notificationId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            int browserId = apiSession.MyMemberId.Value;
            bool result = await notificationsService.RemoveSubscriberNotification(notificationId, browserId);
            if (result)
            {
                Ok();
            }
            else
            {
                NotFound();
            }
        }


        [HttpDelete("RemoveNotification")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task RemoveNotification(long postId)
        {
            await notificationsService.RemoveNotifications(postId);
        }

        [HttpPost("SetReadNotification")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task SetReadNotification(int notificationId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            int browserId = apiSession.MyMemberId.Value;
            await notificationsService.ReadNotification(notificationId, browserId);
        }

        [HttpPost("ReadAllNotifications")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task ReadAllNotifications(List<NotificationMessage> msgs)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            if (apiSession.MyMemberId != null)
            {
                int browserId = apiSession.MyMemberId.Value;
                await notificationsService.ReadAllNotifications(browserId, msgs);
            }
        }
    }
}

using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScSoMe.API.Services;
using ScSoMe.ApiDtos;

namespace ScSoMe.API.Controllers.PushNotificationsController
{
    [ApiController]
    [Route("[controller]")]
    public class PushNotificationsController : ControllerBase
	{

        private readonly ILogger<PushNotificationsController> _logger;
        private readonly EF.ScSoMeContext db;
        private readonly PushedNotification pushNotificationsService;
        public PushNotificationsController(ILogger<PushNotificationsController> logger)
		{
            _logger = logger;
            db = new EF.ScSoMeContext();
            pushNotificationsService = new PushedNotification();
        }


        [HttpPost("CreateMemberDeviceToken")]
        [ProducesResponseType(200)]
        [ProducesResponseType(201)]
        [ProducesResponseType(500)]
        public async Task CreateMemberDeviceToken(string deviceToken, int memberId)
        {
            //var apiSession = new ApiSession(this);
            //apiSession.Check();
            try
            {
                bool exists = await db.MemberDeviceTokens.AnyAsync(x => x.MemberId == memberId && x.DeviceToken.Equals(deviceToken));
                if (!exists)
                {
                    await db.MemberDeviceTokens.AddAsync(new EF.MemberDeviceToken { MemberId = memberId, DeviceToken = deviceToken, LoggedOut = false });
                    await db.SaveChangesAsync();
                    Response.StatusCode = 201;                

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Response.StatusCode = 500;
            }
        }

        [HttpGet("GetTokenForMember")]
        [ProducesResponseType(201)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<string>>> GetTokensForMember(int memberId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            IList<string> result = new List<string>();
            try
            {
                // take first device only; what happens if user has multiple devices?
                var memberDeviceTokens = await db.MemberDeviceTokens.Where(t => t.MemberId == memberId).ToListAsync();
                if (memberDeviceTokens.Count > 0)
                {
                    foreach (var memberDeviseToken in memberDeviceTokens)
                    {
                        if (!string.IsNullOrWhiteSpace(memberDeviseToken.DeviceToken))
                        {
                            Response.StatusCode = 201;
                            result.Add(memberDeviseToken.DeviceToken);
                        }

                    }
                    return Ok(result);
                }
                else
                {
                    return NotFound();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Response.StatusCode = 500;
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("StopPushedNotifications")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> StopPushedNotificationsOnLogout(int memberId, string deviceToken)
        {
            try
            {

                var apiSession = new ApiSession(this);
                apiSession.Check();
                await pushNotificationsService.StopPushedNotificationsOnLogout(memberId, deviceToken);
                return Ok("Pushed notifications stopped");
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        //[HttpPost("ActivatePushedNotifications")]
        //[ProducesResponseType(201)]
        //[ProducesResponseType(500)]
        //public async Task<IActionResult> ActivatePushedNotifications(int memberId, string deviceToken)
        //{
        //    try
        //    {

        //        //var apiSession = new ApiSession(this);
        //        //apiSession.Check();
        //        await pushNotificationsService.ActivatePushedNotificationsOnLogin(memberId, deviceToken);
        //        return Ok();
        //    }
        //    catch (Exception e)
        //    {
        //        return NotFound(e);
        //    }
        //}
    }
}


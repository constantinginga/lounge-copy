using Microsoft.AspNetCore.Mvc;
using ScSoMe.API.Services;

namespace Hangfire.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HangfireController : ControllerBase
    {
        private MemberService memberService;
        public HangfireController()
        {
            memberService = new MemberService();
        }


        [HttpGet(Name = "DailyActiveMembers")]
        [Route("[action]")]
        public IActionResult GetDailyActiveMembers()
        {
            string job = memberService.DailyActiveMembers();
            RecurringJob.AddOrUpdate(() => memberService.DailyActiveMembers(), Cron.Daily);

            return Ok(job);
        }
    }
}
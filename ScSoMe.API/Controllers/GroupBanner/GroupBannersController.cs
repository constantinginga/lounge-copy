using Microsoft.AspNetCore.Mvc;
using ScSoMe.EF;

namespace ScSoMe.API.Controllers.GroupBanner.GroupBannerController
{

    [ApiController]
    [Route("controller")]
    public class GroupBannersController : ControllerBase
    {
        private readonly ILogger<GroupBannersController> _logger;
        private readonly ScSoMeContext db;

        public GroupBannersController(ILogger<GroupBannersController> logger)
        {
            _logger = logger;
            db = new ScSoMeContext();
        }

        [HttpPost("CreateGroupBanner")]
        [ProducesResponseType(201)]
        [ProducesResponseType(500)]
        public int CreateGroupBanner(EF.GroupBanner banner)
        {
            //var apiSession = new ApiSession(this);
            //apiSession.Check();
            try
            {
                var dbGroupBanner = new EF.GroupBanner()
                {
                    Text = banner.Text,
                    ImgUrl = banner.ImgUrl,
                    GroupId = banner.GroupId
                };
                // db.GroupBanners.Add(dbGroupBanner);
                db.SaveChanges();
                Response.StatusCode = 201;
                return dbGroupBanner.Id;

            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                return 0;
            }
        }

        //[HttpPost("EditGroupBanner")]
        //[ProducesResponseType(201)]
        //[ProducesResponseType(500)]
        //public async Task EditGroupBannerText(int id, string newText, string newImageUrl)
        //{
        //    var apiSession = new ApiSession(this);
        //    apiSession.Check();
        //    try
        //    {
        //        var dbGroupBanner = await db.GroupBanners.FindAsync(id);
        //        if (dbGroupBanner == null)
        //        {
        //            NotFound();
        //            return;
        //        }
        //        else
        //        {
        //            dbGroupBanner.Text = newText;
        //            await db.SaveChangesAsync();
        //            Ok();
        //            return;
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        return;
        //    }
        //}
    }
}

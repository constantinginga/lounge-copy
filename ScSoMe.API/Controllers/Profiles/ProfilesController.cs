using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScSoMe.API.Services;
using ScSoMe.ApiDtos;
using ScSoMe.EF;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ScSoMe.API.Controllers.Members.MembersController
{
    [ApiController]
    [Route("[controller]")]
    public class ProfilesController : ControllerBase
    {
        private readonly ScSoMeContext db;
        private readonly ProfileService profileService;

        public ProfilesController()
        {
            db = new ScSoMeContext();
            profileService = new ProfileService();
        }

        [HttpPost("SetProfileDescription")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task SetProfileDescription(int memberId, [FromBody]string description){
            var newDescription = new DescriptionSection{
                MemberId = memberId,
                Content = description,
                PrivacySetting = true,
            };
            await db.DescriptionSections.AddAsync(newDescription);
            await db.SaveChangesAsync();
        }
    }
}
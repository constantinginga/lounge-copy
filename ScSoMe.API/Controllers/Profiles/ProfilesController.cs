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
using Newtonsoft.Json;
using ScSoMe.API.Controllers.Profiles;

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
        public async Task<ProfileResponse> SetProfileDescription([FromBody]JsonElement description){
            DescriptionObject? d = description.Deserialize<DescriptionObject>();
            if(d != null){
                DescriptionSection existingDescription = await profileService.CheckIfMemberHasProfileDescription(d.MemberId);
                ProfileResponse response;
                if(existingDescription != null){
                    existingDescription.Content = d.Description;
                    response = await profileService.UpdateProfileDescription(existingDescription);
                }
                else{
                    response = await profileService.AddProfileDescription(d.MemberId, d.Description);
                }
                return response;
            }
            return new ProfileResponse{
                Message = "Failed to deserialize description object",                    
                StatusCode = HttpStatusCode.InternalServerError,
            };
        }

        [HttpPost("GetProfile")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<Profile> GetProfile([FromBody] string memberId){
            try{
                Profile response = await profileService.GetProfile(Int16.Parse(memberId));
                return response;
            }
            catch(Exception e){
                return null;
            }
        }
    }
}
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
using ScSoMe.API.Controllers.Profiles;
using System.Text.Json.Serialization;

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

        [HttpGet("GetProfile")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<string> GetProfile([FromQuery] int memberId){
            try{
                Member response = await profileService.GetProfile(memberId);
                JsonSerializerOptions options = new()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles,
                    WriteIndented = true
                };
                string serializedResponse = JsonSerializer.Serialize(response, options);
                return serializedResponse;
            }
            catch(Exception e){
                return JsonSerializer.Serialize(new ProfileResponse{
                Message = e.Message,                    
                StatusCode = HttpStatusCode.InternalServerError,});
            }
        }

        [HttpPost("UpdateProfile")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ProfileResponse> UpdateProfile([FromBody]JsonElement profile){
            try{
                Member? newProfile = profile.Deserialize<Member>();
                if (newProfile != null)
                {
                    bool response = await profileService.UpdateProfile(newProfile);
                    if(response){
                        return new ProfileResponse{
                            Message = "Profile updated successfully",                    
                            StatusCode = HttpStatusCode.OK,
                        };
                    }
                    else{
                        return new ProfileResponse{
                            Message = "Failed to update profile",                    
                            StatusCode = HttpStatusCode.InternalServerError,
                        };
                    }
                }
                return new ProfileResponse{
                    Message = "Failed to deserialize received member object",                    
                    StatusCode = HttpStatusCode.InternalServerError,
                };
            }
            catch(Exception e){
                return new ProfileResponse{
                Message = e.Message,                    
                StatusCode = HttpStatusCode.InternalServerError,};
            }
        }
    }
}
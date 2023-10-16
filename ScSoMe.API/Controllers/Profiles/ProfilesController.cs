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

        [HttpPost("GetProfile")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<string> GetProfile([FromBody] string memberId){
            try{
                Member response = await profileService.GetProfile(Int16.Parse(memberId));
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

        [HttpPost("SetServices")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ProfileResponse> SetServices([FromBody]JsonElement services){
            ServiceObject? d = services.Deserialize<ServiceObject>();
            if(d != null){
                ServicesSection existingService = await profileService.CheckIfMemberHasService(d.MemberId);
                ProfileResponse response;
                if(existingService != null){
                    existingService.Content = d.Service;
                    response = await profileService.UpdateService(existingService);
                }
                else{
                    response = await profileService.AddProfileService(d.MemberId, d.Service);
                }
                return response;
            }
            return new ProfileResponse{
                Message = "Failed to deserialize service object",                    
                StatusCode = HttpStatusCode.InternalServerError,
            };
        }

    }
}
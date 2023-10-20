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
        // public async Task<ProfileResponse> UpdateProfile([FromBody]JsonElement profile){
        public async Task<ProfileResponse> UpdateProfile(){
            try{
                // Member? newProfile = profile.Deserialize<Member>();
                Member test = new Member();
                test.MemberId = 1731;
                test.Email = "usdenmarkus@gmail.com";
                test.Name = "Ronni Vien";
                test.DescriptionSection = new DescriptionSection();
                test.DescriptionSection.Content = "This is a test";
                test.ContactsSection = new ContactsSection();
                test.ContactsSection.Email = "This is a test";
                test.ContactsSection.PhoneNumber = 15516546546;
                test.ExternalLinksSection = new ExternalLinksSection();
                test.ExternalLinksSection.ExternalLinks = new List<ExternalLink>();
                test.ExternalLinksSection.ExternalLinks.Add(new ExternalLink { Title = "This is a test", Url = "url testing link" });
                test.ServicesSection = new ServicesSection();
                test.ServicesSection.Content = "This is a test";
                test.ActivitySection = new ActivitySection();
                test.ActivitySection.Content = "This is a test";
                test.WorkExperienceSection = new WorkExperienceSection();
                test.WorkExperienceSection.WorkExperiences = new List<WorkExperience>();
                test.WorkExperienceSection.WorkExperiences.Add(new WorkExperience { CompanyName = "Company testing link", StartDate = DateTimeOffset.Now.DateTime, EndDate = DateTimeOffset.Now.DateTime, Position = "Position testing link", PositionDescription = "Position description testing link" });
                if (test != null)
                {
                    ProfileResponse response = await profileService.UpdateProfile(test);
                    return response;
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
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
                Member response = await profileService.GetProfile(memberId, false);
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

        [HttpGet("GetExternalProfile")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<string> GetExternalProfile([FromQuery] int memberId){
            try{
                Member response = await profileService.GetProfile(memberId, true);
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

        [HttpPost("SyncUser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ProfileResponse> SyncUser([FromBody]JsonElement profile){
            try{
                Member? newProfile = profile.Deserialize<Member>();
                if(newProfile != null){
                    string? response = await profileService.SyncUser(newProfile);
                    if(response != null){
                        return new ProfileResponse{
                            Message = response,                    
                            StatusCode = HttpStatusCode.OK,
                        };
                    }
                    else{
                        return new ProfileResponse{
                            Message = "Failed to sync user",                    
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

        [HttpPost("SyncUserFromUmbraco")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ProfileResponse> SyncUserFromUmbraco([FromBody]JsonElement profile){
            try{
                Member? newProfile = profile.Deserialize<Member>();
                if(newProfile != null){
                    bool response = await profileService.SyncUserFromUmbraco(newProfile);
                    if(response){
                        return new ProfileResponse{
                            Message = response.ToString(),                    
                            StatusCode = HttpStatusCode.OK,
                        };
                    }
                    else{
                        return new ProfileResponse{
                            Message = "Failed to sync user",                    
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

        [HttpGet("CheckToken")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<string> CheckToken([FromQuery] int memberId, [FromQuery] string token){
            try{
                bool success = await profileService.CheckToken(memberId, token);
                if(success){
                    Member response = await profileService.GetProfile(memberId, false);
                    JsonSerializerOptions options = new()
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        WriteIndented = true
                    };
                    string serializedResponse = JsonSerializer.Serialize(response, options);
                    return serializedResponse;
                }
                else{
                    return JsonSerializer.Serialize(new ProfileResponse{
                        Message = "Token is invalid",                    
                        StatusCode = HttpStatusCode.Unauthorized,
                    });
                }
            }
            catch(Exception e){
                return JsonSerializer.Serialize(new ProfileResponse{
                Message = e.Message,                    
                StatusCode = HttpStatusCode.InternalServerError,});
            }
        }

        [HttpPost("AddConnection")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<string> AddConnection([FromQuery] int memberId, [FromQuery] int connectedId){
            try{
                await profileService.AddConnection(memberId, connectedId);
                return JsonSerializer.Serialize(new ProfileResponse{
                    Message = "Succesfully added connection",                    
                    StatusCode = HttpStatusCode.OK,
                });
            }
            catch(Exception e){
                return JsonSerializer.Serialize(new ProfileResponse{
                Message = "Failed to add the connection with error: " + e.Message,                    
                StatusCode = HttpStatusCode.InternalServerError,});
            }
        }

        [HttpPost("RemoveConnection")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<string> RemoveConnection([FromQuery] int memberId, [FromQuery] int connectedId){
            try{
                profileService.RemoveConnection(memberId, connectedId);
                return JsonSerializer.Serialize(new ProfileResponse{
                    Message = "Succesfully removed connection",                    
                    StatusCode = HttpStatusCode.OK,
                });
            }
            catch(Exception e){
                return JsonSerializer.Serialize(new ProfileResponse{
                Message = "Failed to remove connection with error: " + e.Message,                    
                StatusCode = HttpStatusCode.InternalServerError,});
            }
        }
    }
}
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
        public async Task<string> GetProfile([FromQuery] int memberId, [FromQuery] string token){
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
                        Message = "Can't get profile. Token is invalid",                    
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
        public async Task<ProfileResponse> UpdateProfile([FromQuery] int memberId, [FromQuery] string token, [FromBody]JsonElement profile){
            try{
                bool success = await profileService.CheckToken(memberId, token);
                if(success){
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
                else{
                    return new ProfileResponse{
                        Message = "Can't update profile. Token is invalid",                    
                        StatusCode = HttpStatusCode.Unauthorized,
                    };
                }
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
        public async Task<string> AddConnection([FromQuery] int memberId, [FromQuery] string token, [FromBody] JsonElement connection){
            try{
                bool success = await profileService.CheckToken(memberId, token);
                if(success){
                    MemberConnection? newConnection = connection.Deserialize<MemberConnection>();
                    if(newConnection != null){
                        await profileService.AddConnection(newConnection);
                        return JsonSerializer.Serialize(new ProfileResponse{
                            Message = "Succesfully added connection",                    
                            StatusCode = HttpStatusCode.OK,
                        });
                    }
                    else{
                        return JsonSerializer.Serialize(new ProfileResponse{
                            Message = "Failed to deserialize the received connection",                    
                            StatusCode = HttpStatusCode.InternalServerError,});
                    }
                }
                else{
                    return JsonSerializer.Serialize(new ProfileResponse{
                        Message = "Can't add connection. Token is invalid",                    
                        StatusCode = HttpStatusCode.Unauthorized,
                    });
                }
            }
            catch(Exception e){
                return JsonSerializer.Serialize(new ProfileResponse{
                Message = "Failed to add connection with error: " + e.Message,                    
                StatusCode = HttpStatusCode.InternalServerError,});
            }
        }

        [HttpPost("ApproveConnection")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<string> ApproveConnection([FromQuery] int memberId, [FromQuery] string token, [FromBody] JsonElement newConnection){
            try{
                bool success = await profileService.CheckToken(memberId, token);
                if(success){
                    MemberConnection? connection = newConnection.Deserialize<MemberConnection>();
                    if(connection != null){
                        await profileService.ApproveConnection(connection);
                        return JsonSerializer.Serialize(new ProfileResponse{
                            Message = "Succesfully added connection",                    
                            StatusCode = HttpStatusCode.OK,
                        });
                    }
                    else{
                        return JsonSerializer.Serialize(new ProfileResponse{
                            Message = "Failed to deserialize the received connection",                    
                            StatusCode = HttpStatusCode.InternalServerError,});
                    }
                }
                else{
                    return JsonSerializer.Serialize(new ProfileResponse{
                        Message = "Can't approve connection. Token is invalid",                    
                        StatusCode = HttpStatusCode.Unauthorized,
                    });
                
                }
            }
            catch(Exception e){
                return JsonSerializer.Serialize(new ProfileResponse{
                Message = "Failed to approve the connection with error: " + e.Message,                    
                StatusCode = HttpStatusCode.InternalServerError,});
            }
        }

        [HttpPost("RemoveConnection")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<string> RemoveConnection([FromQuery] int memberId, [FromQuery] string token, [FromBody] JsonElement connectionToRemove){
            try{
                bool success = await profileService.CheckToken(memberId, token);
                if(success){
                    MemberConnection? connection = connectionToRemove.Deserialize<MemberConnection>();
                    if(connection != null){
                        profileService.RemoveConnection(connection);
                        return JsonSerializer.Serialize(new ProfileResponse{
                            Message = "Succesfully removed connection",                    
                            StatusCode = HttpStatusCode.OK,
                        });
                    }
                    else{
                        return JsonSerializer.Serialize(new ProfileResponse{
                            Message = "Failed to deserialize the received connection",                    
                            StatusCode = HttpStatusCode.InternalServerError,});
                    }
                }
                else{
                    return JsonSerializer.Serialize(new ProfileResponse{
                        Message = "Can't remove connection. Token is invalid",                    
                        StatusCode = HttpStatusCode.Unauthorized,
                    });
                
                }
            }
            catch(Exception e){
                return JsonSerializer.Serialize(new ProfileResponse{
                Message = "Failed to remove connection with error: " + e.Message,                    
                StatusCode = HttpStatusCode.InternalServerError,});
            }
        }

        [HttpGet("GetConnections")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<string> GetConnections([FromQuery] int memberId, [FromQuery] string token){
            try{
                bool success = await profileService.CheckToken(memberId, token);
                if(success){
                    List<MemberConnection> connections = await profileService.GetMemberConnections(memberId);
                    JsonSerializerOptions options = new()
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        WriteIndented = true
                    };
                    string serializedResponse = JsonSerializer.Serialize(connections, options);
                    return serializedResponse;
                }
                else{
                    return JsonSerializer.Serialize(new ProfileResponse{
                        Message = "Can't get connections. Token is invalid",                    
                        StatusCode = HttpStatusCode.Unauthorized,
                    });
                }
            }
            catch(Exception e){
                return JsonSerializer.Serialize(new ProfileResponse{
                Message = "Failed to get connections with error: " + e.Message,                    
                StatusCode = HttpStatusCode.InternalServerError,});
            }
        }

        [HttpGet("GetConnectionRequests")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<string> GetConnectionRequests([FromQuery] int memberId, [FromQuery] string token){
            try{
                bool success = await profileService.CheckToken(memberId, token);
                if(success){
                    List<MemberConnection> requests = await profileService.GetMemberConnectionRequests(memberId);
                    JsonSerializerOptions options = new()
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        WriteIndented = true
                    };
                    string serializedResponse = JsonSerializer.Serialize(requests, options);
                    return serializedResponse;
                }
                else{
                    return JsonSerializer.Serialize(new ProfileResponse{
                        Message = "Can't get connection requests. Token is invalid",                    
                        StatusCode = HttpStatusCode.Unauthorized,
                    });
                }
            }
            catch(Exception e){
                return JsonSerializer.Serialize(new ProfileResponse{
                Message = "Failed to get connection requests with error: " + e.Message,                    
                StatusCode = HttpStatusCode.InternalServerError,});
            }
        }
    }
}
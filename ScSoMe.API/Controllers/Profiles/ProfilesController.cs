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
                    Services.Profile response = await profileService.GetProfile(memberId, false, false);
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
        public async Task<string> GetExternalProfile([FromQuery] int memberIdToView, [FromQuery] int? authId, [FromQuery] string? token){
            try{
                if(token != null && authId != null){
                    bool success = await profileService.CheckToken((int)authId, token);
                    if(success){
                        bool isConnection = profileService.CheckMemberConnectionById((int)authId, memberIdToView);
                        Services.Profile response = await profileService.GetProfile(memberIdToView, true, isConnection);
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
                else{
                    Services.Profile response = await profileService.GetProfile(memberIdToView, true, false);
                    JsonSerializerOptions options = new()
                    {
                        ReferenceHandler = ReferenceHandler.IgnoreCycles,
                        WriteIndented = true
                    };
                    string serializedResponse = JsonSerializer.Serialize(response, options);
                    return serializedResponse;
                }
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
        public async Task<ProfileResponse> CheckToken([FromQuery] int memberId, [FromQuery] string token){
            try{
                bool success = await profileService.CheckToken(memberId, token);
                if(success){
                    return new ProfileResponse{
                        Message = "Token is valid",                    
                        StatusCode = HttpStatusCode.OK,
                    };
                }
                else{
                    return new ProfileResponse{
                        Message = "Token is invalid",                    
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

        [HttpGet("CheckTokenProfile")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<string> CheckTokenProfile([FromQuery] int memberId, [FromQuery] string token){
            try{
                bool success = await profileService.CheckToken(memberId, token);
                if(success){
                    Services.Profile response = await profileService.GetProfile(memberId, false, false);
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
                bool tokenPassed = await profileService.CheckToken(memberId, token);
                bool isFreeUser = await profileService.CheckIsFreeUser(memberId);
                if(tokenPassed && !isFreeUser){
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
                bool tokenPassed = await profileService.CheckToken(memberId, token);
                bool isFreeUser = await profileService.CheckIsFreeUser(memberId);
                if(tokenPassed && !isFreeUser){
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
                bool tokenPassed = await profileService.CheckToken(memberId, token);
                bool isFreeUser = await profileService.CheckIsFreeUser(memberId);
                if(tokenPassed && !isFreeUser){
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
                bool tokenPassed = await profileService.CheckToken(memberId, token);
                bool isFreeUser = await profileService.CheckIsFreeUser(memberId);
                if(tokenPassed && !isFreeUser){
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
                bool tokenPassed = await profileService.CheckToken(memberId, token);
                bool isFreeUser = await profileService.CheckIsFreeUser(memberId);
                if(tokenPassed && !isFreeUser){
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

        [HttpPost("CheckMemberConnection")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<string> CheckMemberConnection([FromQuery] int memberId, [FromQuery] string token, [FromBody] JsonElement connectionToCheck){
            try{
                bool tokenPassed = await profileService.CheckToken(memberId, token);
                bool isFreeUser = await profileService.CheckIsFreeUser(memberId);
                if(tokenPassed && !isFreeUser){
                    MemberConnection? re = connectionToCheck.Deserialize<MemberConnection>();
                    if(re != null){
                        MemberConnection? requestStatus = await profileService.CheckMemberConnection(re);
                        JsonSerializerOptions options = new()
                        {
                            ReferenceHandler = ReferenceHandler.IgnoreCycles,
                            WriteIndented = true
                        };
                        string serializedResponse = JsonSerializer.Serialize(requestStatus, options);
                        return serializedResponse;
                    }
                    else{
                        return JsonSerializer.Serialize(new ProfileResponse{
                            Message = "Failed to deserialize the received connection",                    
                            StatusCode = HttpStatusCode.InternalServerError,});
                    }
                }
                else{
                    return JsonSerializer.Serialize(new ProfileResponse{
                        Message = "Can't get connection status. Token is invalid",                    
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

        [HttpGet("SearchProfiles")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<string> SearchProfiles([FromQuery] int memberId, [FromQuery] string token, [FromQuery]string terms)
        {
            bool tokenPassed = await profileService.CheckToken(memberId, token);
            if(tokenPassed){
                var result = await profileService.SearchProfiles(terms, memberId);

                string serializedResponse = JsonSerializer.Serialize(result);

                return serializedResponse;
            } else{
                return JsonSerializer.Serialize(new ProfileResponse{
                    Message = "Can't get profile. Token is invalid",                    
                    StatusCode = HttpStatusCode.Unauthorized,
                });
            }
        }
    }
}
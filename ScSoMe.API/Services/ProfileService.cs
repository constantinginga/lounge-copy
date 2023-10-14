using System.Net;
using Microsoft.EntityFrameworkCore;
using ScSoMe.API.Controllers.Members;
using ScSoMe.API.Controllers.Profiles;
using ScSoMe.EF;

namespace ScSoMe.API.Services
{
    public class ProfileService{
        private readonly ScSoMeContext db;

        public ProfileService()
        {
            db = new ScSoMeContext();
        }

        public async Task<ProfileResponse> AddProfileDescription(int memberId, string? description){
            try{
                var newDescription = new DescriptionSection{
                        MemberId = memberId,
                        Content = description,
                        PrivacySetting = true,
                };
                await db.DescriptionSections.AddAsync(newDescription);
                await db.SaveChangesAsync();
                return new ProfileResponse{
                    Message = "Description added",
                    StatusCode = HttpStatusCode.OK,
                };
            }
            catch(Exception e){
                return new ProfileResponse{
                    Message = e.Message,
                    StatusCode = HttpStatusCode.InternalServerError,
                };
            }
        }

        public async Task<ProfileResponse> UpdateProfileDescription(DescriptionSection description){
            try{

                db.DescriptionSections.Update(description);
                await db.SaveChangesAsync();
                return new ProfileResponse{
                    Message = "Description updated",
                    StatusCode = HttpStatusCode.OK,
                };
            }
            catch(Exception e){
                return new ProfileResponse{
                    Message = e.Message,
                    StatusCode = HttpStatusCode.InternalServerError,
                };
            }
        }

        public async Task<DescriptionSection> CheckIfMemberHasProfileDescription(int memberId){
            try{
                var description = await db.DescriptionSections.FirstOrDefaultAsync(d => d.MemberId == memberId);
                if(description != null){
                    return description;
                }
                return null;
            }
            catch(Exception e){
                return null;
            }
        }

        public async Task<Profile> GetProfile(int memberId){
            return null;
        }
    }
}
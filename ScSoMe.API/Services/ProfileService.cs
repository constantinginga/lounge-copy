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

        public async Task<Member> GetProfile(int memberId){
            try{
                var member = await db.Members.FirstAsync(m => m.MemberId == memberId);
                if(member != null){
                    var description = await db.DescriptionSections.FirstOrDefaultAsync(d => d.MemberId == memberId);
                    var contacts = await db.ContactsSections.FirstOrDefaultAsync(c => c.MemberId == memberId);
                    var externalLinks = await db.ExternalLinksSections.FirstOrDefaultAsync(e => e.MemberId == memberId);
                    var services = await db.ServicesSections.FirstOrDefaultAsync(s => s.MemberId == memberId);
                    var activity = await db.ActivitySections.FirstOrDefaultAsync(a => a.MemberId == memberId);
                    var workExperience = await db.WorkExperienceSections.FirstOrDefaultAsync(w => w.MemberId == memberId);
                    member.DescriptionSection = description;
                    member.ContactsSection = contacts;
                    member.ExternalLinksSection = externalLinks;
                    member.ServicesSection = services;
                    member.ActivitySection = activity;
                    member.WorkExperienceSection = workExperience;
                    return member;
                }
                return null;
            }
            catch(Exception e){
                Console.WriteLine("Pepe error" + e);
                return null;
            }
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

        public async Task<ServicesSection> CheckIfMemberHasService(int memberId){
            try{
                var service = await db.ServicesSections.FirstOrDefaultAsync(s => s.MemberId == memberId);
                if(service != null){
                    return service;
                }
                return null;
            }
            catch(Exception e){
                return null;
            }
        }

        public async Task<ProfileResponse> AddProfileService(int memberId, string? service){
            try{
                var newService = new ServicesSection{
                        MemberId = memberId,
                        Content = service,
                        PrivacySetting = true,
                };
                await db.ServicesSections.AddAsync(newService);
                await db.SaveChangesAsync();
                return new ProfileResponse{
                    Message = "Service added",
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

        public async Task<ProfileResponse> UpdateService(ServicesSection service){
            try{
                db.ServicesSections.Update(service);
                await db.SaveChangesAsync();
                return new ProfileResponse{
                    Message = "Service updated",
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
    }
}
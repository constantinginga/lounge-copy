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
                    var externalLinksSection = await db.ExternalLinksSections.FirstOrDefaultAsync(e => e.MemberId == memberId);
                    var externalLinks = await db.ExternalLinks.Where(e => e.MemberId == memberId).ToListAsync();
                    var services = await db.ServicesSections.FirstOrDefaultAsync(s => s.MemberId == memberId);
                    var activity = await db.ActivitySections.FirstOrDefaultAsync(a => a.MemberId == memberId);
                    var workExperienceSection = await db.WorkExperienceSections.FirstOrDefaultAsync(w => w.MemberId == memberId);
                    var workExperiences = await db.WorkExperiences.Where(w => w.MemberId == memberId).ToListAsync();
                    member.DescriptionSection = description;
                    member.ContactsSection = contacts;
                    member.ExternalLinksSection = externalLinksSection;
                    member.ExternalLinksSection.ExternalLinks = externalLinks;
                    member.ServicesSection = services;
                    member.ActivitySection = activity;
                    member.WorkExperienceSection = workExperienceSection;
                    member.WorkExperienceSection.WorkExperiences = workExperiences;
                    return member;
                }
                return null;
            }
            catch(Exception e){
                return null;
            }
        }

        public async Task<ProfileResponse> UpdateProfile(Member newProfile){
            try{
                var oldProfile = await GetProfile(newProfile.MemberId);
                var propertyInfos = typeof(Member).GetProperties();
                foreach (var prop in propertyInfos)
                {
                    if(prop.GetValue(newProfile) != prop.GetValue(oldProfile)){
                        if(prop.GetValue(oldProfile) == null){
                            //Add new value
                            await AddProfileProp(newProfile.MemberId, prop.Name, prop.GetValue(newProfile));
                        }
                        else{
                            //Update value
                            // UpdateProfileProp(newProfile.MemberId, prop.Name, prop.GetValue(newProfile));
                        }
                    }
                    Console.WriteLine("Name: " + prop.Name + ", new Value: " + prop.GetValue(newProfile) + ", old Value: " + prop.GetValue(oldProfile));
                }
                // db.Members.Update(newProfile);
                // await db.SaveChangesAsync();
                return new ProfileResponse{
                    Message = "Profile updated",
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

        public async Task AddProfileProp(int memberId, string prop, Object newVal){
            if(newVal == typeof(DescriptionSection)){

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
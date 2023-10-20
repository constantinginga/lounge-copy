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
                    if(externalLinksSection != null){
                        member.ExternalLinksSection = externalLinksSection;
                        member.ExternalLinksSection.ExternalLinks = new List<ExternalLink>(externalLinks);
                    }
                    member.ServicesSection = services;
                    member.ActivitySection = activity;
                    if(workExperienceSection != null){
                        member.WorkExperienceSection = workExperienceSection;
                        member.WorkExperienceSection.WorkExperiences = new List<WorkExperience>(workExperiences);
                    }
                    return member;
                }
                return null;
            }
            catch(Exception e){
                return null;
            }
        }

        public async Task<bool> UpdateProfile(Member newProfile){
            try{
                var oldProfile = await GetProfile(newProfile.MemberId);
                var propertyInfos = typeof(Member).GetProperties();
                bool response = true;
                foreach (var prop in propertyInfos)
                {
                    if(prop.GetValue(newProfile) != prop.GetValue(oldProfile)){
                        if(prop.GetValue(oldProfile) == null){
                            //Add new value
                            response = await AddProfileProp(newProfile.MemberId, prop.Name, prop.GetValue(newProfile));
                        }
                        else{
                            //Update value
                            response = await UpdateProfileProp(newProfile.MemberId, prop.Name, prop.GetValue(newProfile));
                        }
                    }
                }
                return response;
            }
            catch(Exception e){
                return false;
            }
        }

        public async Task<bool> AddProfileProp(int memberId, string prop, object newVal)
        {
            bool response = true;
            switch (prop)
            {
                case nameof(DescriptionSection):
                    response = await AddProfileDescription(memberId, (newVal as DescriptionSection).Content);
                    break;
                case nameof(ContactsSection):
                    response = await AddProfileContacts(memberId, (newVal as ContactsSection).Email, (newVal as ContactsSection).PhoneNumber);
                    break;
                case nameof(ExternalLinksSection):
                    response = await AddProfileExternalLinksSection(memberId, (newVal as ExternalLinksSection).ExternalLinks.ToList());
                    break;
                case nameof(ServicesSection):
                    response = await AddProfileService(memberId, (newVal as ServicesSection).Content);
                    break;
                case nameof(ActivitySection):
                    response = await AddProfileActivitySection(memberId, (newVal as ActivitySection).Content);
                    break;
                case nameof(WorkExperienceSection):
                    response = await AddProfileWorkExperienceSection(memberId, (newVal as WorkExperienceSection).WorkExperiences.ToList());
                    break;
            }
            await db.SaveChangesAsync();
            return response;
        }

        public async Task<bool> UpdateProfileProp(int memberId, string prop, object newVal)
        {
            bool response = true;
            switch (prop)
            {
                case nameof(DescriptionSection):
                    response = await UpdateProfileDescription((newVal as DescriptionSection));
                    break;
                case nameof(ContactsSection):
                    response = await UpdateContacts((newVal as ContactsSection));
                    break;
                case nameof(ExternalLinksSection):
                    response = await UpdateProfileExternalLinksSection(memberId, (newVal as ExternalLinksSection));
                    break;
                case nameof(ServicesSection):
                    response = await UpdateService((newVal as ServicesSection));
                    break;
                case nameof(ActivitySection):
                    response = await UpdateProfileActivitySection((newVal as ActivitySection));
                    break;
                case nameof(WorkExperienceSection):
                    response = await UpdateProfileWorkExperienceSection(memberId, (newVal as WorkExperienceSection));
                    break;
            }
            await db.SaveChangesAsync();
            return response;
        }

        public async Task<bool> AddProfileDescription(int memberId, string? description){
            try{
                var newDescription = new DescriptionSection{
                        MemberId = memberId,
                        Content = description,
                        PrivacySetting = true,
                };
                await db.DescriptionSections.AddAsync(newDescription);
                return true;
            }
            catch(Exception e){
                return false;
            }
        }

        public async Task<bool> UpdateProfileDescription(DescriptionSection description){
            try{
                db.DescriptionSections.Update(description);
                return true;
            }
            catch(Exception e){
                return false;
            }
        }

        public async Task<bool> AddProfileService(int memberId, string? service){
            try{
                var newService = new ServicesSection{
                        MemberId = memberId,
                        Content = service,
                        PrivacySetting = true,
                };
                await db.ServicesSections.AddAsync(newService);
                return true;
            }
            catch(Exception e){
                return false;
            }
        }

        public async Task<bool> UpdateService(ServicesSection service){
            try{
                db.ServicesSections.Update(service);
                return true;
            }
            catch(Exception e){
                return false;
            }
        }

        public async Task<bool> AddProfileContacts(int memberId, string? email, long? phoneNumber){
            try{
                var newContacts = new ContactsSection{
                        MemberId = memberId,
                        Email = email,
                        PhoneNumber = phoneNumber,
                        PrivacySetting = true,
                };
                await db.ContactsSections.AddAsync(newContacts);
                return true;
            }
            catch(Exception e){
                return false;
            }
        }

        public async Task<bool> UpdateContacts(ContactsSection contacts){
            try{
                db.ContactsSections.Update(contacts);
                return true;
            }
            catch(Exception e){
                return false;
            }
        }

        public async Task<bool> AddProfileExternalLinksSection(int memberId, List<ExternalLink>? externalLinks = null){
            try{
                var newExternalLinksSection = new ExternalLinksSection{
                        MemberId = memberId,
                        PrivacySetting = true,
                };
                await db.ExternalLinksSections.AddAsync(newExternalLinksSection);
                Member member = await GetProfile(memberId);
                if(externalLinks != null){
                    foreach(var externalLink in externalLinks){
                        await AddProfileExternalLink(externalLink);
                    }
                }
                return true;
            }
            catch(Exception e){
                return false;
            }  
        }

        public async Task<bool> UpdateProfileExternalLinksSection(int memberId, ExternalLinksSection externalLinksSection){
           try{
                Member member = await GetProfile(memberId);
                //Remove old work experiences and afterwards add new
                db.ExternalLinks.RemoveRange(member.ExternalLinksSection.ExternalLinks);
                db.ExternalLinksSections.Update(externalLinksSection);
                if(externalLinksSection.ExternalLinks != null){
                    foreach (var externalLink in externalLinksSection.ExternalLinks)
                    {
                        await db.ExternalLinks.AddAsync(externalLink);
                    }
                }
                return true;
            }
            catch(Exception e){
                return false;               
            }
        }

        public async Task<bool> AddProfileExternalLink(ExternalLink externalLink){
            try{
                await db.ExternalLinks.AddAsync(externalLink);
                return true;
            }
            catch(Exception e){
                return false;
            }
        }

        public async Task<bool> AddProfileWorkExperienceSection(int memberId, List<WorkExperience> workExperiences = null){
            try{
                var newWorkExperienceSection = new WorkExperienceSection{
                        MemberId = memberId,
                        PrivacySetting = true,
                };
                await db.WorkExperienceSections.AddAsync(newWorkExperienceSection);
                Member member = await GetProfile(memberId);
                if(workExperiences != null){
                    foreach(var workExperience in workExperiences){
                        await AddProfileWorkExperience(workExperience);
                    }
                }
                return true;
            }
            catch(Exception e){
                return false;
            }            
        }

        public async Task<bool> UpdateProfileWorkExperienceSection(int memberId, WorkExperienceSection workExperienceSection){
            try{
                Member member = await GetProfile(memberId);
                //Remove old work experiences and afterwards add new
                db.WorkExperiences.RemoveRange(member.WorkExperienceSection.WorkExperiences);
                db.WorkExperienceSections.Update(workExperienceSection);
                if(workExperienceSection.WorkExperiences != null){
                    foreach (var workExperience in workExperienceSection.WorkExperiences)
                    {
                        await db.WorkExperiences.AddAsync(workExperience);
                    }
                }
                return true;
            }
            catch(Exception e){
                return false;            
            }
        }

        public async Task<bool> AddProfileWorkExperience(WorkExperience workExperience){
            try{
                await db.WorkExperiences.AddAsync(workExperience);
                return true;
            }
            catch(Exception e){
                return false;
            }
        }

        public async Task<bool> AddProfileActivitySection(int memberId, string? content){
            try{
                var newActivitySection = new ActivitySection{
                        MemberId = memberId,
                        Content = content,
                        PrivacySetting = true,
                };
                await db.ActivitySections.AddAsync(newActivitySection);
                return true;
            }
            catch(Exception e){
                return false;
            }
        }

        public async Task<bool> UpdateProfileActivitySection(ActivitySection activitySection){
            try{
                db.ActivitySections.Update(activitySection);
                return true;
            }
            catch(Exception e){
                return false;              
            }
        }
    }
}
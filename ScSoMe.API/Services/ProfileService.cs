using System.Net;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<bool> CheckToken(int memberId, string token){
            try{
                var member = await db.MemberTokens.FirstAsync(m => m.MemberId == memberId && m.DeviceId == token);
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Check token: " + e.Message);
                // here it returns exception if no token is found, so also return false
                return false;
            }
        }
        /**
            * <summary>Gets a profile from the database</summary>
            * <param name="memberId">The id of the member</param>
            * Checks if the member has profile sections in DB, if not, adds them
            * <returns>A member object</returns>
        */
        public async Task<Member> GetProfile(int memberId){
            try{
                var member = await db.Members.FirstAsync(m => m.MemberId == memberId);
                if(member != null){
                    //Description section
                    var description = await db.DescriptionSections.FirstOrDefaultAsync(d => d.MemberId == memberId);
                    if(description == null){
                        await AddProfileDescription(memberId, "");
                    }
                    else{
                        member.DescriptionSection = description;
                    }
                    //Contacts section
                    var contacts = await db.ContactsSections.FirstOrDefaultAsync(c => c.MemberId == memberId);
                    if(contacts == null){
                        await AddProfileContacts(memberId, "", null);
                    }
                    else{
                        member.ContactsSection = contacts;
                    }
                    //External links section
                    var externalLinksSection = await db.ExternalLinksSections.FirstOrDefaultAsync(e => e.MemberId == memberId);
                    if(externalLinksSection == null){
                        await AddProfileExternalLinksSection(memberId, null);
                    }
                    else{
                        member.ExternalLinksSection = externalLinksSection;
                    }
                    //External links
                    var externalLinks = await db.ExternalLinks.Where(e => e.MemberId == memberId).ToListAsync();
                    member.ExternalLinksSection.ExternalLinks = new List<ExternalLink>(externalLinks);
                    //Services section
                    var services = await db.ServicesSections.FirstOrDefaultAsync(s => s.MemberId == memberId);
                    if(services == null){
                        await AddProfileService(memberId, "");
                    }
                    else{
                        member.ServicesSection = services;
                    }
                    //Activity section
                    var activity = await db.ActivitySections.FirstOrDefaultAsync(a => a.MemberId == memberId);
                    if(activity == null){
                        await AddProfileActivitySection(memberId, "");
                    }
                    else{
                        member.ActivitySection = activity;
                    }
                    //Work experience section
                    var workExperienceSection = await db.WorkExperienceSections.FirstOrDefaultAsync(w => w.MemberId == memberId);
                    if(workExperienceSection == null){
                        await AddProfileWorkExperienceSection(memberId, null);
                    }
                    else{
                        member.WorkExperienceSection = workExperienceSection;
                    }
                    //Work experiences
                    var workExperiences = await db.WorkExperiences.Where(w => w.MemberId == memberId).ToListAsync();
                    member.WorkExperienceSection.WorkExperiences = new List<WorkExperience>(workExperiences);
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
                        response = await UpdateProfileProp(newProfile.MemberId, prop.Name, prop.GetValue(newProfile));
                    }
                }
                response = await UpdateMemberDt(newProfile);
                if(response){
                    await SyncUser(newProfile);
                }
                return response;
            }
            catch(Exception e){
                Console.WriteLine("Update profile: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> AddProfileProp(int memberId, string prop, object? newVal)
        {
            bool response = true;
            switch (prop)
            {
                case nameof(DescriptionSection):
                    response = await AddProfileDescription(memberId, (newVal as DescriptionSection)?.Content);
                    break;
                case nameof(ContactsSection):
                    response = await AddProfileContacts(memberId, (newVal as ContactsSection)?.Email, (newVal as ContactsSection)?.PhoneNumber);
                    break;
                case nameof(ExternalLinksSection):
                    response = await AddProfileExternalLinksSection(memberId, (newVal as ExternalLinksSection)?.ExternalLinks.ToList());
                    break;
                case nameof(ServicesSection):
                    response = await AddProfileService(memberId, (newVal as ServicesSection)?.Content);
                    break;
                case nameof(ActivitySection):
                    response = await AddProfileActivitySection(memberId, (newVal as ActivitySection)?.Content);
                    break;
                case nameof(WorkExperienceSection):
                    response = await AddProfileWorkExperienceSection(memberId, (newVal as WorkExperienceSection)?.WorkExperiences.ToList());
                    break;
            }
            await db.SaveChangesAsync();
            return response;
        }

        public async Task<bool> UpdateProfileProp(int memberId, string prop, object? newVal)
        {
            bool response = true;
            switch (prop)
            {
                case nameof(Member.Name):
                    response = await UpdateProfileName(memberId, (newVal as string));
                    break;
                case nameof(Member.Image):
                    response = await UpdateProfileImage(memberId, (newVal as string));
                    break;
                case nameof(Member.Banner):
                    response = await UpdateProfileBanner(memberId, (newVal as string));
                    break;
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
            return response;
        }

        private readonly string baseUmbracoUrl = "http://localhost:1111";
            // "https://www.startupcentral.dk";

        [HttpPost("SyncUser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<string> SyncUser(Member member){
            HttpClient umbracoApiClient = new HttpClient();
            var uri = new Uri(baseUmbracoUrl + "/umbraco/api/MemberApi/UpdateUserFromLounge");
            var parameters = new Dictionary<string, string>();
            parameters.Add("memberId", member.MemberId.ToString());
            parameters.Add("name", member.Name);
            parameters.Add("phoneNumber", member.ContactsSection.PhoneNumber);

            var postContent = new FormUrlEncodedContent(parameters);
            HttpResponseMessage response = await umbracoApiClient.PostAsync(uri, postContent);
            response.EnsureSuccessStatusCode();
            var resultString = await response.Content.ReadAsStringAsync();
            return resultString;
        }

        public async Task<bool> SyncUserFromUmbraco(Member member){
            var oldProfile = await GetProfile(member.MemberId);
            bool response = true;
            if(oldProfile.Name != member.Name){
                response = await UpdateProfileName(member.MemberId, member.Name);
            }
            if(oldProfile.ContactsSection.PhoneNumber != member.ContactsSection.PhoneNumber){
                response = await UpdateContacts(member.ContactsSection);
            }
            return response;
        }

        public async Task<bool> UpdateMemberDt(Member member){
            try{
                var oldMember = await db.Members.FirstAsync(m => m.MemberId == member.MemberId);
                oldMember.UpdatedDt = DateTime.Now;
                db.ChangeTracker.Clear();
                db.Members.Update(oldMember);
                await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Update memberDt: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> UpdateProfileName(int memberId, string? name){
            try{
                var member = await db.Members.FirstAsync(m => m.MemberId == memberId);
                member.Name = name;
                db.ChangeTracker.Clear();
                db.Members.Update(member);
                await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Update profile name: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> UpdateProfileImage(int memberId, string? imageUrl){
            try{
                var member = await db.Members.FirstAsync(m => m.MemberId == memberId);
                member.Image = imageUrl;
                db.ChangeTracker.Clear();
                db.Members.Update(member);
                await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Update profile image: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> UpdateProfileBanner(int memberId, string? bannerUrl){
            try{
                var member = await db.Members.FirstAsync(m => m.MemberId == memberId);
                member.Banner = bannerUrl;
                db.ChangeTracker.Clear();
                db.Members.Update(member);
                await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Update profile banner: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> AddProfileDescription(int memberId, string? description){
            try{
                var newDescription = new DescriptionSection{
                        MemberId = memberId,
                        Content = description,
                        PrivacySetting = true,
                };
                await db.DescriptionSections.AddAsync(newDescription);
                await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> UpdateProfileDescription(DescriptionSection description){
            try{
                db.ChangeTracker.Clear();
                db.DescriptionSections.Update(description);
                await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Update profile description: " + e.Message);
                throw new Exception(e.Message);
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
                await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> UpdateService(ServicesSection service){
            try{
                db.ChangeTracker.Clear();
                db.ServicesSections.Update(service);
                await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Update service: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> AddProfileContacts(int memberId, string? email, string? phoneNumber){
            try{
                var newContacts = new ContactsSection{
                        MemberId = memberId,
                        Email = email,
                        PhoneNumber = phoneNumber,
                        PrivacySetting = true,
                };
                await db.ContactsSections.AddAsync(newContacts);
                await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> UpdateContacts(ContactsSection contacts){
            try{
                db.ChangeTracker.Clear();
                db.ContactsSections.Update(contacts);
                await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Update contacts: " + e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> AddProfileExternalLinksSection(int memberId, List<ExternalLink>? externalLinks = null){
            try{
                var newExternalLinksSection = new ExternalLinksSection{
                        MemberId = memberId,
                        PrivacySetting = true,
                };
                await db.ExternalLinksSections.AddAsync(newExternalLinksSection);
                await db.SaveChangesAsync();
                Member member = await GetProfile(memberId);
                if(externalLinks != null){
                    foreach(var externalLink in externalLinks){
                        externalLink.MemberId = memberId;
                        await AddProfileExternalLink(externalLink);
                    }
                }
                return true;
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
                throw new Exception(e.Message);
            }  
        }

        public async Task<bool> UpdateProfileExternalLinksSection(int memberId, ExternalLinksSection externalLinksSection){
           try{
                Member member = await GetProfile(memberId);
                db.ChangeTracker.Clear();
                db.Entry(externalLinksSection).CurrentValues.SetValues(externalLinksSection);
                if(externalLinksSection.ExternalLinks != null){
                    db.ExternalLinks.RemoveRange(member.ExternalLinksSection.ExternalLinks);
                    db.SaveChanges();

                    db.ExternalLinks.AddRange(externalLinksSection.ExternalLinks);
                    db.SaveChanges();
                }
                db.SaveChanges();
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Update external links: " + e.Message + " : " + e.StackTrace);
                throw new Exception(e.Message);              
            }
        }

        public async Task<bool> AddProfileExternalLink(ExternalLink externalLink){
            try{
                await db.ExternalLinks.AddAsync(externalLink);
                await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> AddProfileWorkExperienceSection(int memberId, List<WorkExperience> workExperiences = null){
            try{
                var newWorkExperienceSection = new WorkExperienceSection{
                        MemberId = memberId,
                        PrivacySetting = true,
                };
                await db.WorkExperienceSections.AddAsync(newWorkExperienceSection);
                await db.SaveChangesAsync();
                Member member = await GetProfile(memberId);
                if(workExperiences != null){
                    foreach(var workExperience in workExperiences){
                        await AddProfileWorkExperience(workExperience);
                    }
                }
                return true;
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
                throw new Exception(e.Message);
            }            
        }

        public async Task<bool> UpdateProfileWorkExperienceSection(int memberId, WorkExperienceSection workExperienceSection){
            try{
                Member member = await GetProfile(memberId);
                db.ChangeTracker.Clear();
                db.Entry(workExperienceSection).CurrentValues.SetValues(workExperienceSection);
                if(workExperienceSection.WorkExperiences != null){
                    db.WorkExperiences.RemoveRange(member.WorkExperienceSection.WorkExperiences);
                    db.SaveChanges();

                    db.WorkExperiences.AddRange(workExperienceSection.WorkExperiences);
                    db.SaveChanges();
                }
                db.SaveChanges();
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Update work experience: " + e.Message + " : " + e.InnerException);
                throw new Exception(e.Message);           
            }
        }

        public async Task<bool> AddProfileWorkExperience(WorkExperience workExperience){
            try{
                await db.WorkExperiences.AddAsync(workExperience);
                await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> AddProfileActivitySection(int memberId, string? content){
            try{
                // var newActivitySection = new ActivitySection{
                //         MemberId = memberId,
                //         Content = content,
                //         PrivacySetting = true,
                // };
                // await db.ActivitySections.AddAsync(newActivitySection);
                // await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> UpdateProfileActivitySection(ActivitySection activitySection){
            try{
                // db.ActivitySections.Update(activitySection);
                // await db.SaveChangesAsync();
                return true;
            }
            catch(Exception e){
                Console.WriteLine("Update profile activity: " + e.Message);
                throw new Exception(e.Message);             
            }
        }

        public async Task<MemberConnection> GetConnection(int memberId, int connectedId){
            try{
                var connection = await db.MemberConnections.FirstAsync(m => m.MemberId == memberId && m.ConnectedId == connectedId || m.MemberId == connectedId && m.ConnectedId == memberId);
                return connection;
            }
            catch(Exception e){
                Console.WriteLine(e.Message + " " + e.InnerException);
                throw new Exception(e.Message);
            }
        }

        public async Task AddConnection(int memberId, int connectedId){
            try{
                var newConnection = new MemberConnection{
                        MemberId = memberId,
                        ConnectedId = connectedId,
                };
                await db.MemberConnections.AddAsync(newConnection);
                await db.SaveChangesAsync();
            }
            catch(Exception e){
                Console.WriteLine(e.Message + " " + e.InnerException);
                throw new Exception(e.Message);
            }
        }
        public async void RemoveConnection(int memberId, int connectedId){
            try{
                var connectionToRemove = await GetConnection(memberId, connectedId);
                db.ChangeTracker.Clear();
                db.MemberConnections.Remove(connectionToRemove);
                db.SaveChanges();
            }
            catch(Exception e){
                Console.WriteLine(e.Message + " " + e.InnerException);
                throw new Exception(e.Message);
            }
        }

        public async Task<List<MemberConnection>> GetMemberConnections(int memberId){
            try{
                var connections = await db.MemberConnections.Where(m => m.MemberId == memberId || m.ConnectedId == memberId).ToListAsync();
                return connections;
            }
            catch(Exception e){
                Console.WriteLine(e.Message + " " + e.InnerException);
                throw new Exception(e.Message);
            }
        }
    }
}
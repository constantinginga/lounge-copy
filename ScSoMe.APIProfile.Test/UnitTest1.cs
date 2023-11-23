using Azure;
using NUnit.Framework;
using ScSoMe.API.Services;
using ScSoMe.EF;

namespace ScSoMe.APIProfile.Test{
    [TestFixture]
    public class Tests
    {
        private ProfileService _profileService;

        /**
            Requires connected ScSoMe database to run successfully
        */
        [SetUp]
        public void Setup()
        {
            _profileService = new ProfileService();
        }

        [TearDown]
        public void TearDown()
        {
            _profileService = null;
        }

        [Test]
        public async Task CheckTokenSuccess()
        {
            int memberId = 28283;
            string token = "0586684e-91ab-44da-ac74-e0c9ee1265e6";
            bool response = await _profileService.CheckToken(memberId, token);
            if(response){
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task CheckTokenFail()
        {
            int memberId = 28283;
            string token = "0586684e-92ab-44da-ac74-e0c9ee1265e6";
            bool response = await _profileService.CheckToken(memberId, token);
            if(response){
                Assert.Fail();
            }
            else{
                Assert.Pass();
            }
        }

        [Test]
        public async Task GetProfileSuccess()
        {
            Member testMember = CreateTestMemberInDB();
            API.Services.Profile response = await _profileService.GetProfile(testMember.MemberId, false);
            RemoveTestMemberFromDB(testMember);
            if(response != null && response.member.Name.Equals(testMember.Name)){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task GetProfileFail()
        {
            API.Services.Profile response = await _profileService.GetProfile(-69, false);
            if(response.member == null){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task UpdateProfileContactsSectionSuccess()
        {
            Member testMember = CreateTestMemberInDB();
            testMember.ContactsSection.Email = "test2";
            bool response = await _profileService.UpdateContacts(testMember.ContactsSection);
            API.Services.Profile updatedMember = await _profileService.GetProfile(testMember.MemberId, false);
            RemoveTestMemberFromDB(testMember);
            if(response && updatedMember.member.ContactsSection.Email.Equals(testMember.ContactsSection.Email)){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task UpdateProfileContactsSectionPassingNull()
        {
            Member testMember = CreateTestMemberInDB();
            testMember.ContactsSection.Email = null;
            bool response = await _profileService.UpdateContacts(testMember.ContactsSection);
            RemoveTestMemberFromDB(testMember);
            if(response){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task UpdateProfileDescriptionSectionSuccess()
        {
            Member testMember = CreateTestMemberInDB();
            testMember.DescriptionSection.Content = "test2";
            bool response = await _profileService.UpdateProfileDescription(testMember.DescriptionSection);
            API.Services.Profile updatedMember = await _profileService.GetProfile(testMember.MemberId, false);
            RemoveTestMemberFromDB(testMember);
            if(response && updatedMember.member.DescriptionSection.Content.Equals(testMember.DescriptionSection.Content)){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task UpdateProfileDescriptionSectionPassingNull()
        {
            Member testMember = CreateTestMemberInDB();
            testMember.DescriptionSection.Content = null;
            bool response = await _profileService.UpdateProfileDescription(testMember.DescriptionSection);
            RemoveTestMemberFromDB(testMember);
            if(response){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task UpdateProfileServicesSectionSuccess()
        {
            Member testMember = CreateTestMemberInDB();
            testMember.ServicesSection.Content = "test2";
            bool response = await _profileService.UpdateService(testMember.ServicesSection);
            API.Services.Profile updatedMember = await _profileService.GetProfile(testMember.MemberId, false);
            RemoveTestMemberFromDB(testMember);
            if(response && updatedMember.member.ServicesSection.Content.Equals(testMember.ServicesSection.Content)){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task UpdateProfileServicesSectionPassingNull()
        {
            Member testMember = CreateTestMemberInDB();
            testMember.ServicesSection.Content = null;
            bool response = await _profileService.UpdateService(testMember.ServicesSection);
            RemoveTestMemberFromDB(testMember);
            if(response){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task UpdateProfileExternalLinksOne()
        {
            Member testMember = CreateTestMemberInDB();
            testMember.ExternalLinksSection.ExternalLinks.Add(new ExternalLink{
                ExternalLinkId = "testtesttest",
                MemberId = -69,
                Title = "test",
                Url = "test",
            });
            bool response = await _profileService.UpdateProfileExternalLinksSection(testMember.MemberId, testMember.ExternalLinksSection);
            API.Services.Profile updatedMember = await _profileService.GetProfile(testMember.MemberId, false);
            RemoveTestMemberFromDB(testMember);
            if(response && updatedMember.member.ExternalLinksSection.ExternalLinks.Count == testMember.ExternalLinksSection.ExternalLinks.Count){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task UpdateProfileExternalLinksMany()
        {
            Member testMember = CreateTestMemberInDB();
            testMember.ExternalLinksSection.ExternalLinks.Add(new ExternalLink{
                ExternalLinkId = "testtesttest",
                MemberId = -69,
                Title = "test",
                Url = "test",
            });
            testMember.ExternalLinksSection.ExternalLinks.Add(new ExternalLink{
                ExternalLinkId = "testtesttest2",
                MemberId = -69,
                Title = "test2",
                Url = "test2",
            });
            bool response = await _profileService.UpdateProfileExternalLinksSection(testMember.MemberId, testMember.ExternalLinksSection);
            API.Services.Profile updatedMember = await _profileService.GetProfile(testMember.MemberId, false);
            RemoveTestMemberFromDB(testMember);
            if(response && updatedMember.member.ExternalLinksSection.ExternalLinks.Count == testMember.ExternalLinksSection.ExternalLinks.Count){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task UpdateProfileExternalLinksZero()
        {
            Member testMember = CreateTestMemberInDB();
            testMember.ExternalLinksSection.ExternalLinks.Add(new ExternalLink{
                ExternalLinkId = "testtesttest",
                MemberId = -69,
                Title = "test",
                Url = "test",
            });
            testMember.ExternalLinksSection.ExternalLinks.Add(new ExternalLink{
                ExternalLinkId = "testtesttest2",
                MemberId = -69,
                Title = "test2",
                Url = "test2",
            });
            await _profileService.UpdateProfileExternalLinksSection(testMember.MemberId, testMember.ExternalLinksSection);
            API.Services.Profile updatedMember = await _profileService.GetProfile(testMember.MemberId, false);
            updatedMember.member.ExternalLinksSection.ExternalLinks.Clear();
            bool response = await _profileService.UpdateProfileExternalLinksSection(testMember.MemberId, testMember.ExternalLinksSection);
            RemoveTestMemberFromDB(testMember);
            if(response && updatedMember.member.ExternalLinksSection.ExternalLinks.Count == testMember.ExternalLinksSection.ExternalLinks.Count){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task UpdateProfileWorkExperiencesOne()
        {
            Member testMember = CreateTestMemberInDB();
            testMember.WorkExperienceSection.WorkExperiences.Add(new WorkExperience{
                WorkExperienceId = "testtesttest",
                MemberId = -69,
                CompanyName = "test",
                Position = "test",
                PositionDescription = "test",
                StartDate = new System.DateTime(2020, 1, 1),
                EndDate = new System.DateTime(2020, 1, 1),
            });
            bool response = await _profileService.UpdateProfileWorkExperienceSection(testMember.MemberId, testMember.WorkExperienceSection);
            API.Services.Profile updatedMember = await _profileService.GetProfile(testMember.MemberId, false);
            RemoveTestMemberFromDB(testMember);
            if(response && updatedMember.member.WorkExperienceSection.WorkExperiences.Count == testMember.WorkExperienceSection.WorkExperiences.Count){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task UpdateProfileWorkExperiencesMany()
        {
            Member testMember = CreateTestMemberInDB();
             testMember.WorkExperienceSection.WorkExperiences.Add(new WorkExperience{
                WorkExperienceId = "testtesttest",
                MemberId = -69,
                CompanyName = "test",
                Position = "test",
                PositionDescription = "test",
                StartDate = new System.DateTime(2020, 1, 1),
                EndDate = new System.DateTime(2020, 1, 1),
            });
             testMember.WorkExperienceSection.WorkExperiences.Add(new WorkExperience{
                WorkExperienceId = "testtesttest2",
                MemberId = -69,
                CompanyName = "test2",
                Position = "test2",
                PositionDescription = "test2",
                StartDate = new System.DateTime(2020, 1, 1),
                EndDate = new System.DateTime(2020, 1, 1),
            });
            bool response = await _profileService.UpdateProfileWorkExperienceSection(testMember.MemberId, testMember.WorkExperienceSection);
            API.Services.Profile updatedMember = await _profileService.GetProfile(testMember.MemberId, false);
            RemoveTestMemberFromDB(testMember);
            if(response && updatedMember.member.WorkExperienceSection.WorkExperiences.Count == testMember.WorkExperienceSection.WorkExperiences.Count){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        [Test]
        public async Task UpdateProfileWorkExperiencesZero()
        {
            Member testMember = CreateTestMemberInDB();
            testMember.WorkExperienceSection.WorkExperiences.Add(new WorkExperience{
                WorkExperienceId = "testtesttest",
                MemberId = -69,
                CompanyName = "test",
                Position = "test",
                PositionDescription = "test",
                StartDate = new System.DateTime(2020, 1, 1),
                EndDate = new System.DateTime(2020, 1, 1),
            });
             testMember.WorkExperienceSection.WorkExperiences.Add(new WorkExperience{
                WorkExperienceId = "testtesttest2",
                MemberId = -69,
                CompanyName = "test2",
                Position = "test2",
                PositionDescription = "test2",
                StartDate = new System.DateTime(2020, 1, 1),
                EndDate = new System.DateTime(2020, 1, 1),
            });
            await _profileService.UpdateProfileWorkExperienceSection(testMember.MemberId, testMember.WorkExperienceSection);
            API.Services.Profile updatedMember = await _profileService.GetProfile(testMember.MemberId, false);
            updatedMember.member.WorkExperienceSection.WorkExperiences.Clear();
            bool response = await _profileService.UpdateProfileWorkExperienceSection(testMember.MemberId, testMember.WorkExperienceSection);
            RemoveTestMemberFromDB(testMember);
            if(response && updatedMember.member.WorkExperienceSection.WorkExperiences.Count == testMember.WorkExperienceSection.WorkExperiences.Count){ 
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }

        /** HELPING METHODS **/
        Member CreateTestMemberInDB(){
            Member testMember = new Member();
            testMember.MemberId = -69;
            testMember.Name = "Test";
            testMember.Email = "";
            testMember.Password = "";
            testMember.Login = "";
            testMember.Url = "";
            testMember.CreatedDt = DateTimeOffset.Now;
            testMember.UpdatedDt = DateTimeOffset.Now;
            testMember.ContactsSection = new ContactsSection{
                MemberId = -69,
                Email = "test@test.com",
                PhoneNumber = "+6969696969",
                PrivacySetting = false,
            };
            testMember.DescriptionSection = new DescriptionSection{
                MemberId = -69,
                Content = "Test",
                PrivacySetting = false,
            };
            testMember.ServicesSection = new ServicesSection{
                MemberId = -69,
                PrivacySetting = false,
            };
            testMember.ExternalLinksSection = new ExternalLinksSection{
                MemberId = -69,
                PrivacySetting = false,
            };
            testMember.WorkExperienceSection = new WorkExperienceSection{
                MemberId = -69,
                PrivacySetting = false,
            };
            using (var context = new ScSoMeContext())
            {
                context.ChangeTracker.Clear();
                context.Members.Add(testMember);
                context.ContactsSections.Add(testMember.ContactsSection);
                context.ServicesSections.Add(testMember.ServicesSection);
                context.DescriptionSections.Add(testMember.DescriptionSection);
                context.ExternalLinksSections.Add(testMember.ExternalLinksSection);
                context.WorkExperienceSections.Add(testMember.WorkExperienceSection);
                context.SaveChanges();
            }
            return testMember;
        }

        void RemoveTestMemberFromDB(Member testMember){
            using (var context = new ScSoMeContext())
            {
                context.ChangeTracker.Clear();
                context.ContactsSections.Remove(testMember.ContactsSection);
                context.ServicesSections.Remove(testMember.ServicesSection);
                context.DescriptionSections.Remove(testMember.DescriptionSection);
                context.ExternalLinksSections.Remove(testMember.ExternalLinksSection);
                context.ExternalLinks.RemoveRange(testMember.ExternalLinksSection.ExternalLinks);
                context.WorkExperienceSections.Remove(testMember.WorkExperienceSection);
                context.WorkExperiences.RemoveRange(testMember.WorkExperienceSection.WorkExperiences);
                context.Members.Remove(testMember);
                context.SaveChanges();
            }
        }
    }
}
using NUnit.Framework;
using ScSoMe.API.Services;
using ScSoMe.EF;

namespace ScSoMe.APIProfile.Test{
    [TestFixture]
    public class Tests
    {
        private ProfileService _profileService;

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
        public async Task Test1()
        {
            int id = 28283;   
            Member response = await _profileService.GetProfile(id, false);
            if(response != null && response.MemberId == id){
                Assert.Pass();
            }
            else{
                Assert.Fail();
            }
        }
    }
}
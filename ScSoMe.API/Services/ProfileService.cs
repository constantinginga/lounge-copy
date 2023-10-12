using Microsoft.EntityFrameworkCore;
using ScSoMe.API.Controllers.Members;
using ScSoMe.EF;

namespace ScSoMe.API.Services
{
    public class ProfileService{
        private readonly ScSoMeContext db;

        public ProfileService()
        {
            db = new ScSoMeContext();
        }

        public void AddDescription(int memberId, string description){
            try{
                Console.WriteLine("Inserting description: " + description + " for member: " + memberId);
            }
            catch(Exception ex){
                Console.WriteLine(ex);
            }
        }
    }
}
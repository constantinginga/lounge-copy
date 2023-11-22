using ScSoMe.EF;

namespace ScSoMe.API.Services{

    public partial class Profile
    {
        public Member member { get; set; }
        public CustomActivitySection activitySection { get; set; }
    }
 }
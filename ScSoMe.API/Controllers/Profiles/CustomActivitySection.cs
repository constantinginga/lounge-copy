using ScSoMe.EF;

namespace ScSoMe.API.Services{

    public partial class CustomActivitySection : ActivitySection
    {
        public CustomActivitySection()
        {
            ActivityGroups = new HashSet<ActivityLevel>();
        }
        public int MemberId { get; set; }
        public string? Name { get; set; }
        public DateTimeOffset? JoinDate { get; set; }
        public long? NumberOfMentions { get; set; }
        public bool? PrivacySetting { get; set; }
        public virtual ICollection<ActivityLevel> ActivityGroups { get; set; }
    }
 }
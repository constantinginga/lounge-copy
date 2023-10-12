using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class WorkExperienceSection
    {
        public WorkExperienceSection()
        {
            WorkExperiences = new HashSet<WorkExperience>();
        }

        public int MemberId { get; set; }
        public bool? PrivacySetting { get; set; }

        public virtual Member Member { get; set; } = null!;
        public virtual ICollection<WorkExperience> WorkExperiences { get; set; }
    }
}

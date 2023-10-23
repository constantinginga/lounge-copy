using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class WorkExperience
    {
        public string WorkExperienceId { get; set; } = null!;
        public int? MemberId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CompanyName { get; set; }
        public string? Position { get; set; }
        public string? PositionDescription { get; set; }

        public virtual WorkExperienceSection? Member { get; set; }
    }
}

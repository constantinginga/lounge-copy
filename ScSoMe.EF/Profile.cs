using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class Profile
    {
        public int MemberId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DescriptionSection DescriptionSection { get; set; }
        public ContactsSection ContactsSection { get; set; }
        public ExternalLinksSection ExternalLinksSection { get; set; }
        public ServicesSection ServicesSection { get; set; }
        public ActivitySection ActivitySection { get; set; }
        public WorkExperienceSection WorkExperienceSection { get; set; }
        public bool isMember { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class ContactsSection
    {
        public int MemberId { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool? PrivacySetting { get; set; }

        public virtual Member Member { get; set; } = null!;
    }
}

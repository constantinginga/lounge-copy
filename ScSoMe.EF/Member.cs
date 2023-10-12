﻿using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class Member
    {
        public Member()
        {
            Participants = new HashSet<Participant>();
        }

        public int MemberId { get; set; }
        public DateTimeOffset CreatedDt { get; set; }
        public DateTimeOffset UpdatedDt { get; set; }
        public string Name { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Url { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Json { get; set; }

        public virtual ICollection<Participant> Participants { get; set; }
    }
}

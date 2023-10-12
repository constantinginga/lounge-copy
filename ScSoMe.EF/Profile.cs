using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class Profile
    {
        public int MemberId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public bool isMember { get; set; }
    }
}
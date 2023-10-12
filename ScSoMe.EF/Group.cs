using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class Group
    {
        public Group()
        {
            Comments = new HashSet<Comment>();
        }

        public int GroupId { get; set; }
        public DateTimeOffset CreatedDt { get; set; }
        public DateTimeOffset UpdatedDt { get; set; }
        public string Groupname { get; set; } = null!;
        public string Url { get; set; } = null!;

        public virtual ICollection<Comment> Comments { get; set; }
    }
}

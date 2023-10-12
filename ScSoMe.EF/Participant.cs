using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class Participant
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int MemberId { get; set; }

        public virtual Chat Chat { get; set; } = null!;
        public virtual Member Member { get; set; } = null!;
    }
}

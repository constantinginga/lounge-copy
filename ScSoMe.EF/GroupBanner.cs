using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class GroupBanner
    {
        public int Id { get; set; }
        public long GroupId { get; set; }
        public string? Text { get; set; }
        public string? ImgUrl { get; set; }
    }
}

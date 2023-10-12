using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class Translation
    {
        public long CommentId { get; set; }
        public string LanguageCode { get; set; } = null!;
        public string? TranslatedComment { get; set; }
    }
}

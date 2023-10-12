using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class Emoji
    {
        public int EmojiId { get; set; }
        public string? EmojiIcon { get; set; }
        public string? Description { get; set; }
        public string Category { get; set; } = null!;
    }
}

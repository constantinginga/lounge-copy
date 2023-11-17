using System;
using System.Collections.Generic;

namespace ScSoMe.EF
{
    public partial class Comment
    {
        public long CommentId { get; set; }
        /// <summary>
        /// Null for the post - and post id for every comment
        /// </summary>
        public long? RootCommentId { get; set; }
        public int GroupId { get; set; }
        public long? ParentCommentId { get; set; }
        public int AuthorMemberId { get; set; }
        public DateTimeOffset CreatedDt { get; set; }
        public DateTimeOffset UpdatedDt { get; set; }
        public string? Text { get; set; }
        public string? LikersJson { get; set; }
        public string? EmbeddedUrl { get; set; }
        public bool? HasMedia { get; set; }
        public bool? PrivacySetting { get; set; }

        public virtual Group Group { get; set; } = null!;
    }
}

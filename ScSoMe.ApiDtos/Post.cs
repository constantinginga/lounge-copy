namespace ScSoMe.ApiDtos
{
    public class WriteMessage
    {
        public long Id { get; set; }
        //public int AuthorMemberId { get; set; }
        public string Text { get; set; } = null!;
    }

    public class Comment    
    {
        public long Id { get; set; }
        public int AuthorMemberId { get; set; }
        public DateTimeOffset UpdatedDt { get; set; }
        public DateTimeOffset CreatedDt { get; set; }
        public string Text { get; set; } = null!;
        public IDictionary<int,int>? LikeType2Count { get; set; }
        public int? BrowserLikeType { get; set; }
        public List<Comment> Responses { get; set; }
        public bool? HasMedia { get; set; }
        public bool? PrivacySetting { get; set; }
    }

    public class Post : Comment
    {
        public int GroupId { get; set; }
    }
}
namespace ScSoMe.API.Controllers.Comments.CommentsController
{
    public class MatchingPost
    {
        public long PostId { get; set; }

        public long MessageId { get; set; }

        public int GroupId { get; set; }

        public DateTime DateTime { get; set; }
    }
}
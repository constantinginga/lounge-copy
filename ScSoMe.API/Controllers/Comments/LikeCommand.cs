namespace ScSoMe.API.Controllers.Comments
{
    public class LikeCommand
    {
        public long MessageId { get; set; }
        public int LikeType { get; set; }
    }
}

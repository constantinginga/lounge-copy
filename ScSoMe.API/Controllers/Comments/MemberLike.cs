namespace ScSoMe.API.Controllers.Comments
{
    public class MemberLike
    {
        public string MemberName { get; set; } = string.Empty;
        public int MemberId { get; set; }
        public int LikeType { get; set; }
    }
}

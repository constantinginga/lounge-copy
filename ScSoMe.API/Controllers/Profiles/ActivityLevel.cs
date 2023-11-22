 namespace ScSoMe.API.Services{

    public partial class ActivityLevel
    {
        public int MemberId { get; set; }
        public string? Name { get; set; }
        public long? NumberOfPosts { get; set; }
        public long? NumberOfComments { get; set; }
        public long? NumberOfLikes { get; set; }
    }
 }
 
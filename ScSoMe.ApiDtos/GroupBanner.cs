namespace ScSoMe.ApiDtos
{
    public class GroupBanner
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string? Text { get; set; }
        public string? ImgUrl { get; set; }
    }
}

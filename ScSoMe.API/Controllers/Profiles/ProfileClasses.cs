using System.Net;

namespace ScSoMe.API.Controllers.Profiles
{
    public class ProfileJson
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public bool isMember { get; set; }
    }

    public class ProfileResponse{
        public string Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }

    public class DescriptionObject{
        public int MemberId { get; set; }
        public string? Description { get; set; }
    }
}
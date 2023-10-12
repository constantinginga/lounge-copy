namespace ScSoMe.API.Controllers.Members
{
    public class LoginResult
    {
        public bool Success { get; set; }
        public bool? AccountLockedOut { get; set; }
        public int MemberId { get; set; }
        public string Token { get; set; }
        public int LeftLoginAttempts { get; set; }
    }
}

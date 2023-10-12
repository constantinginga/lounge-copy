namespace ScSoMe.API.Controllers.Members
{
    public class Login
    {
        public string Username { get; set; }
        public string ClearTextPassword { get; set; }
        public string CrossSessionUniqueClientID { get; set; }
    }
}

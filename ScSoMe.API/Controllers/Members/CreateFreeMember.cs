namespace ScSoMe.API.Controllers.Members
{
    public class CreateFreeMember
    {
        public string Name { get; set; }
        public string ClearTextPassword { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? Cvr { get; set; }
    }
}

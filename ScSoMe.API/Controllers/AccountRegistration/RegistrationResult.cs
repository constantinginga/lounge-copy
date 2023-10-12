namespace ScSoMe.API.Controllers.AccountRegistration
{
    public class RegistrationResult
    {
        public RegistrationResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
        public bool Success { get; set; }
        public string? Message { get; set; }

    }
}

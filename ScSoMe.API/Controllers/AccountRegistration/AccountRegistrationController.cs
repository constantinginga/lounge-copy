using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScSoMe.ApiDtos;
using ScSoMe.EF;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Diagnostics;


namespace ScSoMe.API.Controllers.AccountRegistration.AccountRegistrationController
{
    [ApiController]
    [Route("[controller]")]
    public class AccountRegistrationController : ControllerBase
    {
        private readonly ScSoMeContext? db = new ScSoMeContext();
        

        private string RegistrationValidation(Registration registration)
        {
            Debug.WriteLine("Password is1:" + registration.Password);
            if (string.IsNullOrEmpty(registration.Email))
            {
                Debug.WriteLine(registration.Email);
                return "Email cannot be empty";
            }
            string loginRules = @"^(?=[a-zA-Z0-9._]{6,20}$)(?!.*[_.]{2})[^_.].*[^_.]$";
            if (!Regex.IsMatch(registration.Login, loginRules))
            {
                Debug.WriteLine("loginRules");
                return "Username is not valid!";
            }
            if (db.Members.Any(_ => _.Login.ToLower() == registration.Login.ToLower()))
            {
                Debug.WriteLine("User Exists");
                return "A user with that name already exists!";
            }
            string emailRules = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
            if (!Regex.IsMatch(registration.Email, emailRules))
            {
                Debug.WriteLine("Email not valid");
                return "Not a valid Email!";
            }
            if (db.Members.Any(_ => _.Email.ToLower() == registration.Email.ToLower()))
            {
                Debug.WriteLine("Email exists");
                return "A user with that Email already exists!";
            }
            if (string.IsNullOrEmpty(registration.Password) || string.IsNullOrEmpty(registration.ConfirmPassword))
            {
                Debug.WriteLine("Password empty");
                return "Password or Password confirmation cannot be empty";
            }
            if (registration.Password != registration.ConfirmPassword)
            {
                Debug.WriteLine("Wrong confirmation");
                return "Invalid password confirmation";
            }
            string passwordRules = @"^.*(?=.{8,})(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!*@#$%^&+=]).*$";

            if (!Regex.IsMatch(registration.Password, passwordRules))
            {
                Debug.WriteLine("Password is2:" + registration.Password);
                return "The password should contain atleast: 1 Uppercase, 1 Lower case," +
                    " 1 Special character, 1 number and should be a minimum length of 8 characters";
            }
            Debug.WriteLine("Password is:" + registration.Password + "2");
            return string.Empty;
        }
        [NonAction]
        private string PasswordHash(string password)
        {
            byte[] salt = new byte[16];

            new RNGCryptoServiceProvider().GetBytes(salt);
            var passwordRfc = new Rfc2898DeriveBytes(password, salt, 1000);
            byte[] hash = passwordRfc.GetBytes(20);
            byte[] hashBtye = new byte[36];
            Array.Copy(salt, 0, hashBtye, 0, 16);
            Array.Copy(hash, 0, hashBtye, 16, 20);

            return Convert.ToBase64String(hashBtye);
        }
        [NonAction]
        public int DecrementMemberId()
        {
            int lastMemberId = 0;
            lastMemberId = db.Members.Min(x => x.MemberId);
            return --lastMemberId;
        }

        
        [HttpPost("UserRegistration")]
        public async Task<RegistrationResult> UserRegistrationAsync([FromBody] Registration registration)
        {

            DateTimeOffset currentDateTime = new DateTimeOffset(DateTime.UtcNow);
            string message = RegistrationValidation(registration);
            var result = new RegistrationResult(false, message);
            if (!string.IsNullOrEmpty(message))
            {
                return result;
            }

            Member newMember = new()
            {
                MemberId = DecrementMemberId(),
                Email = registration.Email,
                Name = registration.FirstName + " " + registration.LastName,
                Login = registration.Login,
                Url = registration.FirstName + "." + registration.LastName,
                Password = PasswordHash(registration.Password),
                CreatedDt = currentDateTime,
            };
            db.Members.Add(newMember);
            result.Success = true;
            await db.SaveChangesAsync();
            Ok();
            return result;
        }

        [HttpPost("GetCountryCodes")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(405)]
        [ProducesResponseType(500)]
        public async Task<IList<CountryCode>> GetCountryCodes()
        {
           return await db.CountryCodes.ToListAsync();
        }



    }
}


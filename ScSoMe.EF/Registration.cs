using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScSoMe.EF
{
    public class Registration
    {
        public Registration(int memberId, string? firstName, string? lastName, string? email, string? password, string? confirmPassword, string? login)
        {
            MemberId = memberId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Password = password;
            ConfirmPassword = confirmPassword;
            Login = login;

        }
        public Registration()
        {

        }
        public int MemberId { get; set; }
        public string? Login { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }


    }
}

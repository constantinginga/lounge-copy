global using API;

namespace ScSoMe.Common
{
    public class ApiClientFactory
    {
        public int MemberId { get; private set; } = 0;
        private readonly HttpClient hc = new HttpClient();
        public string DeviceType { get; set; } = string.Empty;
        public bool IsMAUI { get { return !string.Empty.Equals(DeviceType); } }
        public static string BaseUrl { get; set; }
            = Environment.MachineName.Equals("startupVM") ? "https://api.startupcentral.dk" :
        //"https://testapi.startupcentral.dk";
        "https://localhost:7297";
        //public static string BaseUrl { get; set; } = "https://testapi.startupcentral.dk";
        public static string MauiBaseUrl { get; set; } = "https://api.startupcentral.dk";

        private ScSoMeApi? client = null;


        string? Token { get; set; }

        public API.ScSoMeApi GetApiClient()
        {
            if (client == null)
            {
                if (DeviceType.Equals("Android") || DeviceType.Equals("IOS"))// || DeviceType.Equals("WinUI"))
                {
                    client = new ScSoMeApi(MauiBaseUrl, hc);
                }
                else
                {
                    client = new ScSoMeApi(BaseUrl, hc);
                }
            }

            return client;
        }


        public void SetDeviceType(string typeOfDevice)
        {
            DeviceType = typeOfDevice;
        }

        /// <summary>
        /// Check that returned loginResult.Success == true
        /// </summary>
        /// <param name="username"></param>
        /// <param name="clearTextPassword"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<LoginResult> LoginAsync(string username, string clearTextPassword, string crossSessionUniqueClientID)
        {
            client = GetApiClient();
            API.LoginResult loginResult = null;
            try
            {
                loginResult = await client.LoginByUsernameAsync(
                    new Login { Username = username, ClearTextPassword = clearTextPassword, CrossSessionUniqueClientID = crossSessionUniqueClientID });
            }
            catch (Exception ex)
            {
                ex.ToString();
                System.Diagnostics.Debugger.Break();
                throw;// new Exception("Login error", ex);
            }

            if (loginResult.MemberId != 0) // && loginResult.MemberId != memberId)
            {
                SetToken(loginResult.Token);
            }

            MemberId = loginResult.MemberId;

            return loginResult;
        }

        public void SetToken(string token)
        {
            hc.DefaultRequestHeaders.Remove("X-ScSoMe-Token");
            hc.DefaultRequestHeaders.Add("X-ScSoMe-Token", new[] { token });
            Token = token;
            if (MemberId == 0)
            {
                MemberId = GetApiClient().GetMyMemberInfoAsync().GetAwaiter().GetResult().Id;
            }
        }

        public string GetAutoSignInUrlForPlatform()
        {
            // http://localhost:1111/?nl_token=5835a615-efc5-41a2-910a-8a644b303eda&memberId=26502
            var url = $"https://www.startupcentral.dk/?nl_token={Token}&memberId={MemberId}";
            return url;
        }

        public async Task<bool> SignOut()
        {
            bool signoutResult = false;

            signoutResult = await client.SignOutFromTokenAsync(Token);
            hc.DefaultRequestHeaders.Remove("X-ScSoMe-Token");
            client = null;
            MemberId = 0;
            return signoutResult;
        }


        public async Task<RegistrationResult> RegistrationAsync(string login, string firstName, string lastName, string email, string password, string confirmPassword)
        {
            if (client == null)
                client = new ScSoMeApi(BaseUrl, hc);
            API.RegistrationResult registration = null;
            try
            {
                registration = await client.UserRegistrationAsync(new Registration
                {
                    Login = login,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    Password = password,
                    ConfirmPassword = confirmPassword
                });
            }

            catch (Exception ex)
            {
                ex.ToString();
                System.Diagnostics.Debugger.Break();
                throw new Exception("Registration error", ex);
            }
            return registration;

        }

    }
}



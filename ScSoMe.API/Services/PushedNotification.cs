using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ScSoMe.API.Controllers.Members;
using ScSoMe.EF;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ScSoMe.API.Services
{
    public class PushedNotification
    {
        private string _firebaseKey;
        private readonly string _firebaseUrl = "https://fcm.googleapis.com/fcm/send";
        private readonly IConfiguration _configuration;
        public PushedNotification()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }
        
        public async Task<string> SendNotification(string title, string body, string[] deviceTokens)
        {
            _firebaseKey = _configuration.GetConnectionString("FirebaseKey");
            var client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "key=" + _firebaseKey);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

            var payload = new
            {
                registration_ids = deviceTokens,
                notification = new
                {
                    title = title,
                    body = body,
                    sound = "default",
                    badge = "1"
                },
                data = new
                {
                    NavigationID = 2,
                }
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_firebaseUrl, content);
            var result = await response.Content.ReadAsStringAsync();
            //UIApplication.SharedApplication.ApplicationIconBadgeNumber++;

            Console.WriteLine(result);
                return result;
                

        }


        public async Task PrepareDeviceNotificaion(string title, string body, int memberId)
        {
            var db = new ScSoMeContext();
            var subscribersDevices = db.MemberDeviceTokens.Where(x => x.MemberId == memberId).ToList();
            //var subscribersDevices = db.MemberDeviceTokens.Where(x => x.MemberId == memberId && x.LoggedOut == false).ToList();
            string[] deviceTokens = new string[subscribersDevices.Count];
            for (int i = 0; i < subscribersDevices.Count; i++)
            {
                deviceTokens[i] = subscribersDevices[i].DeviceToken;
            }
            if (deviceTokens.Length > 0)
            {
                await SendNotification(title, body, deviceTokens);
            }

        }
        public async Task StopPushedNotificationsOnLogout(int memberId, string deviceToken)
        {
            var db = new ScSoMeContext();
            var memberDevices = await db.MemberDeviceTokens.Where(x => x.MemberId == memberId).ToListAsync();
            if (memberDevices.Count > 0)
            {
                var memberDevice =  memberDevices.FirstOrDefault(x => x.DeviceToken == deviceToken);
                if (memberDevice != null)
                {
                    memberDevice.LoggedOut = true;
                    await db.SaveChangesAsync();
                }

            }
        }
        
        //public async Task ActivatePushedNotificationsOnLogin(int memberId, string deviceToken)
        //{
        //    var db = new ScSoMeContext();
        //    var record = await db.MemberDeviceTokens.FirstOrDefaultAsync(x => x.MemberId == memberId && x.DeviceToken == deviceToken);
        //    if (record != null)
        //    {
        //        record.LoggedOut = false; 
        //        await db.SaveChangesAsync();
        //    }
        //}

    }
}

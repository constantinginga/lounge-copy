using API;
using NUnit.Framework;
using System.Diagnostics;
using ScSoMe.EF;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace ScSoMe.API.Test
{
    public class Tests
    {
        Process apiProcess;
        private string baseUrl = "http://localhost:5000"; // https://localhost:5001
        ScSoMeApi client;
        int m1 = -1;
        int m2 = -2;
        int g1 = 1;

        [OneTimeSetUp]
        public void Setup()
        {
            var codeBase = this.GetType().Assembly.CodeBase;
            var path = codeBase.Replace("file:///", "")
                    .Replace(@"ScSoMe.API.Test/bin/Debug/net6.0/ScSoMe.API.Test.dll", "")
                    .Replace("/", @"\") + @"ScSoMe.API\bin\Debug\net6.0";
            var psi = new ProcessStartInfo()
            {
                WorkingDirectory = path,
                Arguments = "",
                UseShellExecute = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Maximized,
                FileName = path + "\\ScSoMe.API.exe",
                ErrorDialog = true,                
            };
            this.apiProcess = Process.Start(psi);

            var httpClient = new System.Net.Http.HttpClient();            

            client = new ScSoMeApi(baseUrl, httpClient);

            var db = new ScSoMeContext();
            var members = db.Members.Select(x=>x.MemberId < 0);
            if (members.Count() != 2)
            {
                var now = DateTimeOffset.Now;
                //db.Members.Add(new Member { MemberId = -1, Name = "Minus1", CreatedDt = now, UpdatedDt = now, Email = "", Login = "-1", Password = "", Url = "-1"});
                //db.Members.Add(new Member { MemberId = -2, Name = "Minus2", CreatedDt = now, UpdatedDt = now, Email = "", Login = "-2", Password = "", Url = "-2" });
            }
            db.SaveChanges();

            int i = 0;
            while (true) // wait for API to be ready for usage
            {
                if (i++ > 100) break;
                var delayTask = Task.Delay(123);
                var groupTask = client.GetGroupsAsync();
                Task.WaitAny(delayTask, groupTask);
                if (groupTask.IsCompleted) break;
            }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            apiProcess.Kill(true);
            apiProcess.Close();
            apiProcess.Dispose();
        }

        //[Test]
        //public async Task TestPostCommentsLikeAndGet()
        //{
        //    var acf = new ScSoMe.Common.ApiClientFactory();
        //    ScSoMe.Common.ApiClientFactory.BaseUrl = baseUrl;
        //    await acf.LoginAsync("larsoutzen@gmail.com", "123123", "28");
        //    client = acf.GetApiClient();


        //    var httpClient2 = new System.Net.Http.HttpClient();
        //    ScSoMeApi client2 = new ScSoMeApi(baseUrl, httpClient2);
        //    var loginResult2 = await client2.LoginByUsernameAsync( // to set memberId and pass the Checks
        //        new Login()
        //        {
        //            Username = "lars_outzen@hotmail.com",
        //            ClearTextPassword = "123123"
        //        });
        //    httpClient2.DefaultRequestHeaders.Add("X-ScSoMe-Token", new[] { loginResult2.Token });


        //    var sw = Stopwatch.StartNew();
        //    // arrange and act
        //    var p1 = await client.CreatePostAsync(g1, new WriteMessage() { Text = "Test first Post" });

        //    var p1c1 = await client2.CreateCommentAsync(p1, p1, g1, new WriteMessage() { Text = "L1 - Test first comment" });

        //    var p1c2 = await client2.CreateCommentAsync(p1, p1, g1, new WriteMessage() { Text = "L1 - Test second comment" });

        //    var p1c2c1 = await client.CreateCommentAsync(p1c2, p1, g1, new WriteMessage() { Text = "L2 - Test third comment" });

        //    var t1 = client.LikeMsgAsync(new LikeCommand() { LikeType = 1, MessageId = p1c2c1 });
        //    var t2 = client2.LikeMsgAsync(new LikeCommand() { LikeType = 1, MessageId = p1c2c1 });
        //    var t3 = client2.LikeMsgAsync(new LikeCommand() { LikeType = 1, MessageId = p1 });
        //    var t4 = client2.LikeMsgAsync(new LikeCommand() { LikeType = 1, MessageId = p1 }); // UnLike
        //    var t5 = client.LikeMsgAsync(new LikeCommand() { LikeType = 1, MessageId = p1 });
        //    Task.WaitAll(t1, t2, t3, t4, t5);
        //    Console.WriteLine("9 changes in ms:" + sw.ElapsedMilliseconds);

        //    // apparently the changes are not returned in responses unless we delay the verification a bit
        //    Task.Delay(765).GetAwaiter().GetResult();
        //    sw.Restart();
        //    var posts = await client.GetLatestPostsForGroupAsync(g1, fromDate: DateTimeOffset.Now, daysBack: 99, includeAllComments: true);
        //    Console.WriteLine("Get all posts with comments in ms:" + sw.ElapsedMilliseconds);

        //    // assert
        //    var newest = posts.First();
        //    Assert.AreEqual(p1, newest.Id, "Post are listed with the newest first");

        //    Assert.AreEqual(newest.Responses.First().Id, p1c1);
        //    Assert.AreEqual(newest.Responses.Last().Id, p1c2, "Comments are listed in the sequence they are made");
        //    Assert.AreEqual(newest.Responses.First().BrowserLikeType, null);
        //    Assert.AreEqual(newest.Responses.Last().BrowserLikeType, null);

        //    var p1c2c1msg = newest.Responses.Last().Responses.Single();
        //    Assert.AreEqual(p1c2c1msg.BrowserLikeType, 1);
        //    Assert.AreEqual(p1c2c1msg.AuthorMemberId, (await client.GetMyMemberInfoAsync()).Id );

        //    Assert.AreEqual(p1c2c1msg.LikeType2Count.Count, 1);
        //    Assert.AreEqual(2, p1c2c1msg.LikeType2Count["1"], "Both m1 and m2 liked this comment - using the same LikeType 1");

        //    //await client.LikeMsgAsync(new LikeCommand() { AuthorMemberId = m2, LikeType = 1, MessageId = p1 });
                        
        //    Assert.AreEqual(1, newest.LikeType2Count["1"], "m1 like it and m2 UNliked this Post - using the same LikeType 1");

        //    await client.EditTextAsync(new WriteMessage() { Id = p1, Text = "Edited text in first message" });
        //    var newP1 = await client.GetPostWithCommentsAsync(p1);
        //    Assert.AreEqual("Edited text in first message", newP1.Text);


        //    var p1c2read = await client.GetCommentWithoutChildrenAsync(p1c2);
        //    Assert.IsEmpty(p1c2read.Responses);
        //    Assert.IsNotEmpty(newP1.Responses.Skip(1).Take(1).Single().Responses);

        //    await client.DeleteMessageAndAllChildrenAsync(p1c2c1);
        //    newP1 = await client.GetPostWithCommentsAsync(p1);
        //    Assert.IsEmpty(newP1.Responses.Skip(1).Take(1).Single().Responses);

        //    await client.DeleteMessageAndAllChildrenAsync(p1);
        //    try
        //    {
        //        await client.GetPostWithCommentsAsync(p1);
        //        Assert.Fail();
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.AreEqual("Not Found\n\nStatus: 404\nResponse: \n", ex.Message);
        //    }

        //    Assert.Pass();
        //}

        //[Test]
        public void TestDecodeToken()
        {
            var stream = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiI5YjBhNjcyMi1mOTc4LTRhOWYtYTMyMS1lYjkwOWMwOTY5ZGIiLCJzdWIiOiJkYjdjNDQ0Ni00NmEwLTQyMGEtYmRlMS02ZGQ2YWRhMjdjNjIiLCJleHAiOjE2NzA0OTA3OTAsImlzcyI6Imh0dHBzOi8vd3d3LnN0YXJ0dXBjZW50cmFsLmRrIiwiYXVkIjoiaHR0cHM6Ly93d3cuc3RhcnR1cGNlbnRyYWwuZGsifQ.UZehmyFKEXdchLr7ePT_itYK2qb1D27UolcLLO-9Jyk";
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = handler.ReadToken(stream) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;

            var actor = token.Actor;

            foreach(var claim in token.Claims)
            {
                Console.WriteLine(claim.Subject + " : " + claim.Value);
            }

        }

        [Test]
        public async Task TestLogin()
        {            
            try
            {
                var me = await client.GetMyMemberInfoAsync();
            } catch (Exception ex)
            {                                
                Assert.AreEqual("Server Error\n\nStatus: 500\nResponse: \n", ex.Message);
                ApiException ae = ex as ApiException;
                Assert.AreEqual(500, ae.StatusCode);                
            }

            ScSoMe.Common.ApiClientFactory.BaseUrl = baseUrl;
            var acf = new ScSoMe.Common.ApiClientFactory();
            await acf.LoginAsync("larsoutzen@gmail.com", "123123", "28");
            var c = acf.GetApiClient();

            var me2 = await c.GetMyMemberInfoAsync();
            Assert.AreEqual("Lars Outzen", me2.Name);

            var posts = await c.GetLatestPostsForGroupAsync(g1, fromDate: DateTimeOffset.Now, daysBack: 99, includeAllComments: false);
            var newest = posts.First();

            return;
            var likers = await c.GetLikersAsync(newest.Id);
            Assert.AreEqual(1, likers.Count);
        }


        [Test]
        public async Task TestMessage()
        {
            try
            {
                var me = await client.GetMyMemberInfoAsync();
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Server Error\n\nStatus: 500\nResponse: \n", ex.Message);
                ApiException ae = ex as ApiException;
                Assert.AreEqual(500, ae.StatusCode);
            }

            ScSoMe.Common.ApiClientFactory.BaseUrl = baseUrl;
            var acf = new ScSoMe.Common.ApiClientFactory();
            await acf.LoginAsync("larsoutzen@gmail.com", "123123", "28");
            var c = acf.GetApiClient();

            var me2 = await c.GetMyMemberInfoAsync();
            Assert.AreEqual("Lars Outzen", me2.Name);

            var posts = await c.GetLatestPostsForGroupAsync(g1, fromDate: DateTimeOffset.Now, daysBack: 99, includeAllComments: false);
            var newest = posts.First();

            return;
            var likers = await c.GetLikersAsync(newest.Id);
            Assert.AreEqual(1, likers.Count);
        }

        //Database needs to be cleaned after tests due to it populating the server with dummy data
        [Test]
        public async Task TestGetLastMessage()
        {
            try
            {
                var me = await client.GetMyMemberInfoAsync();
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Server Error\n\nStatus: 500\nResponse: \n", ex.Message);
                ApiException ae = ex as ApiException;
                Assert.AreEqual(500, ae.StatusCode);
            }

            ScSoMe.Common.ApiClientFactory.BaseUrl = baseUrl;
            var acf = new ScSoMe.Common.ApiClientFactory();
            await acf.LoginAsync("293152@via.dk", "12341234", "29");
            var c = acf.GetApiClient();

            await c.CreateOnetoOneAsync("30647,30648", 30647, 30648, "Faustas Volkovas,Mihai Anghelus");
            await c.SaveMessageAsync("30647,30648", "TEST MESAGE FROM TEST", 30647, "Faustas Volkovas");
            var latest = await c.GetLastChatMessageAsync("30647,30648");

            Assert.AreEqual("TEST MESAGE FROM TEST", latest.Message);
        }

        [Test]
        public async Task TestGetTotalUnreadCount()
        {
            try
            {
                var me = await client.GetMyMemberInfoAsync();
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Server Error\n\nStatus: 500\nResponse: \n", ex.Message);
                ApiException ae = ex as ApiException;
                Assert.AreEqual(500, ae.StatusCode);
            }

            ScSoMe.Common.ApiClientFactory.BaseUrl = baseUrl;
            var acf = new ScSoMe.Common.ApiClientFactory();
            await acf.LoginAsync("293169@via.dk", "12341234", "29");
            var c = acf.GetApiClient();

            await c.CreateOnetoOneAsync("30648,30649", 30648, 30649, "Mihai Anghelus,Flavius-Alin Boanca");

            await c.SaveMessageAsync("30647,30648", "1", 30647, "Faustas Volkovas");
            await c.SaveMessageAsync("30647,30648", "2", 30647, "Faustas Volkovas");
            await c.SaveMessageAsync("30648,30649", "3", 30649, "Flavius-Alin Boanca");

            var mihaiCount = await c.GetTotalUnreadMessageCountAsync(30648);
            Assert.AreEqual(4, mihaiCount);
        }

        [Test]
        public async Task TestGetUnreadCount()
        {
            try
            {
                var me = await client.GetMyMemberInfoAsync();
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Server Error\n\nStatus: 500\nResponse: \n", ex.Message);
                ApiException ae = ex as ApiException;
                Assert.AreEqual(500, ae.StatusCode);
            }

            ScSoMe.Common.ApiClientFactory.BaseUrl = baseUrl;
            var acf = new ScSoMe.Common.ApiClientFactory();
            await acf.LoginAsync("293169@via.dk", "12341234", "29");
            var c = acf.GetApiClient();

            var count = await c.GetUnreadMessageCountAsync("30648,30647", 30648);
            Assert.AreEqual(10, count);
        }

        [Test]
        public async Task TestGetAllChats()
        {
            try
            {
                var me = await client.GetMyMemberInfoAsync();
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Server Error\n\nStatus: 500\nResponse: \n", ex.Message);
                ApiException ae = ex as ApiException;
                Assert.AreEqual(500, ae.StatusCode);
            }
            
            ScSoMe.Common.ApiClientFactory.BaseUrl = baseUrl;
            var acf = new ScSoMe.Common.ApiClientFactory();
            await acf.LoginAsync("293152@via.dk", "12341234", "29");
            var c = acf.GetApiClient();

            var allChats = await c.GetAllChatsByUserAsync(30647);
            Assert.AreEqual(9,allChats.Count());
        }


    }
}
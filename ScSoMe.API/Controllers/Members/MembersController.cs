using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScSoMe.API.Services;
using ScSoMe.ApiDtos;
using ScSoMe.EF;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;


namespace ScSoMe.API.Controllers.Members.MembersController
{
    [ApiController]
    [Route("[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly ILogger<MembersController> _logger;
        private readonly ScSoMeContext db;
        private DateTime now = DateTime.Now;


        internal static readonly ConcurrentDictionary<int, UmbracoMemberInfo> cache_memberId2info = new ConcurrentDictionary<int, UmbracoMemberInfo>(1, 4000);
        internal static readonly ConcurrentDictionary<string, int> cache_token2memberId = new ConcurrentDictionary<string, int>();
        private readonly MemberService memberService;

        public MembersController(ILogger<MembersController> logger)
        {
            _logger = logger;
            db = new ScSoMeContext();
            memberService = new MemberService();
        }

        [HttpGet("GetLoginFromValidTokenAndMemberId")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public string GetLoginFromValidTokenAndMemberId(string token, int memberId)
        {
            Response.StatusCode = 404;
            Task.Delay(500).GetAwaiter().GetResult();

            var foundMemberId = 0;
            if (!cache_token2memberId.Any())
            {
                var tokenRow = db.MemberTokens.SingleOrDefault(x => x.Token == token);
                if (tokenRow != null)
                {
                    foundMemberId = tokenRow.MemberId;
                }
            }
            else
            {
                if (!cache_token2memberId.TryGetValue(token, out foundMemberId))
                    return null;
            }

            if (foundMemberId != memberId)
                return null;

            if (!cache_memberId2info.Any())
            {
                MembersController.UpdateCache();
            }
            if (!cache_memberId2info.TryGetValue(memberId, out UmbracoMemberInfo umbracoMemberInfo))
                return null;

            Response.StatusCode = 200;
            Ok();

            return umbracoMemberInfo.Login;
            //FormsAuthentication.SetAuthCookie(loginemail, true);
        }

        [HttpPost("SetMemberIsApproved")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task SetMemberIsApproved(int memberId, bool isApproved)
        {
            var member = await db.Members.Where(m => m.MemberId == memberId).FirstOrDefaultAsync();
            if (member != null)
            {
                var um = cache_memberId2info[memberId];
                if (um != null)
                {
                    um.IsApproved = isApproved;
                    member.Json = JsonSerializer.Serialize(um);
                    await db.SaveChangesAsync();
                }
            }
        }

        [HttpGet("GetMemberEmailSubscriptions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<EmailSubscriptionService.EmailSubscriptions> GetMemberEmailSubscriptions()
        {
            var session = new ApiSession(this);
            session.Check();
            int memberId = session.MyMemberId.Value;
            var service = new EmailSubscriptionService();
            return await service.GetSubscriptions(memberId);
        }

        [HttpGet("SetMemberEmailSubscriptions")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task SetMemberEmailSubscriptions(EmailSubscriptionService.EmailSubscriptions subscription)
        {
            var session = new ApiSession(this);
            session.Check();
            int memberId = session.MyMemberId.Value;
            var service = new EmailSubscriptionService();
            await service.SetSubscriptions(memberId, subscription);
        }

        [HttpGet("GetMemberInfo")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public MemberInfo GetMemberInfo(string shortUrl)
        {
            new ApiSession(this).Check();
            try
            {
                if (string.IsNullOrEmpty(shortUrl))
                {
                    Response.StatusCode = 404;
                    return null;
                };
                var url = shortUrl.ToLowerInvariant();
                var dbMember = db.Members.FirstOrDefault(x => x.Url.Equals(url));
                if (dbMember == null)
                {
                    Response.StatusCode = 404;
                    return null;
                }

                var result = new MemberInfo()
                {
                    Id = dbMember.MemberId,
                    Name = dbMember.Name
                };
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "On shortUrl: {shortUrl}", shortUrl);
                Response.StatusCode = 500;
                return null;
            }

        }


        [HttpGet("GetMemberInfoById")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<MemberInfo> GetMemberInfoById(int memberId)
        {
            try
            {
                //var um = await db.Members.FindAsync(memberId);
                var um = cache_memberId2info[memberId];
                if (um == null)
                {
                    Response.StatusCode = 404;
                    return null;
                }

                return new MemberInfo()
                {
                    Id = memberId,
                    Name = um.Name,
                    CreateDate = um.CreateDate,
                    UpdateDate = um.UpdateDate,
                    Email = um.Email,
                    Username = um.Login,
                    Avatar = um.Avatar,
                    Alias = um.ContentType,
                    IsAdmin = um.IsAdmin,
                    IsApproved = um.IsApproved

                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"On memberId: {memberId}");
                Response.StatusCode = 500;
                return null;
            }
        }



        private readonly string baseUmbracoUrl = "http://localhost:1111";
            // "https://www.startupcentral.dk";

        [HttpPost("GetMyMemberInfo")]
        [ProducesResponseType(200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<MemberInfo> GetMyMemberInfo()
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            var memberId = apiSession.MyMemberId;
            if (null == memberId)
            {
                Response.StatusCode = 403;
                return null;
            }

            Ok();

            var um = cache_memberId2info[memberId.Value];
            return new MemberInfo()
            {
                Id = memberId.Value,
                Name = um.Name,
                CreateDate = um.CreateDate,
                UpdateDate = um.UpdateDate,
                Email = um.Email,
                Username = um.Login,
                Avatar = um.Avatar,
                Alias = um.ContentType,
                IsAdmin = um.IsAdmin,
                IsApproved = um.IsApproved,

            };
        }

        [HttpPost("SignOutFromToken")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<bool> SignOutUsingToken([FromBody] string token)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();

            var dbToken = db.MemberTokens.SingleOrDefault(x => x.Token == token);
            if (dbToken != null)
            {
                db.Remove(dbToken);
                cache_token2memberId.TryRemove(token, out var memberId);
                return true;
            }
            return false;
        }

        [HttpPost("RememberMeAutoLogin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task RememberMeAutoLogin()
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            //TODO: What if member no longer IsApproved? Rotate the token daily and check at the same time??
            var now = DateTime.Now;
            memberService.AddActiveMember(apiSession.MyMemberId.Value, now);
            memberService.DailyActiveMembers();
            Ok();
        }


        [HttpPost("LoginByUsername")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<LoginResult> LoginByUsername([FromBody] Login login)
        {
            _logger.LogInformation("LoginByUsername: " + login.Username);

            login.Username = login.Username.ToLowerInvariant();
            var now = DateTime.Now;
            var result = new LoginResult() { Success = false };

            /*
            if (login.Username.StartsWith("+") /*hack to be disabled* / )
            {
                if (!MembersController.cache_memberId2info.Any()) MembersController.UpdateCache();

                var um = cache_memberId2info.Values.FirstOrDefault(x => x.Login.ToLowerInvariant() == login.Username.Substring(1));
                if (um == null) throw new Exception("Missing member in cache");
                if (!um.IsApproved) throw new Exception("Member is not approved?");                
                result.MemberId = um.Id;

                result.Success = true;

                // TODO: Improve lookup ??
                result.Token = cache_token2memberId.FirstOrDefault(x => x.Value.Equals(um.Id)).Key;
                if (result.Token == null) throw new Exception("Missing token");

                memberService.AddActiveMember(result.MemberId, now);
                memberService.DailyActiveMembers();

                Ok();
                return result;
            }
            */

            HttpClient umbracoApiClient = new HttpClient();
            var uri = new Uri(baseUmbracoUrl + "/umbraco/api/MemberApi/LoginByUsername");
            var parameters = new Dictionary<string, string>();
            parameters.Add("username", login.Username);
            parameters.Add("password", login.ClearTextPassword);
            var postContent = new FormUrlEncodedContent(parameters);
            HttpResponseMessage response = await umbracoApiClient.PostAsync(uri, postContent);
            response.EnsureSuccessStatusCode(); //throw if httpcode is an error
            var resultString = await response.Content.ReadAsStringAsync();

            if (resultString.Equals("true"))
            {
                //AddMembersToDB();

                var dbMember = await db.Members.FirstOrDefaultAsync(x => x.Login.Equals(login.Username)); //&& x.Json.Contains("IsApproved\":true"));
                if (dbMember == null)
                {
                    AddMembersToDB();
                    dbMember = await db.Members.SingleOrDefaultAsync(x => x.Login.Equals(login.Username));
                    // && x.Json.Contains("IsApproved\":true"));
                    if (dbMember == null)
                    {
                        result.Success = false;
                        result.AccountLockedOut = null;
                        return result;
                    }
                }

                result.MemberId = dbMember.MemberId;
                result.Success = true;

                memberService.AddActiveMember(result.MemberId, now);
                memberService.DailyActiveMembers();


                result.Token = Guid.NewGuid().ToString();
                var tokenAdded = cache_token2memberId.TryAdd(result.Token, dbMember.MemberId);
                if (!tokenAdded) throw new Exception("Impossible! Try again?? Existing token: " + result.Token);
                var existingDeviceMemberToken = await db.MemberTokens.SingleOrDefaultAsync(
                    x => x.DeviceId == login.CrossSessionUniqueClientID);
                if (existingDeviceMemberToken != null)
                {
                    existingDeviceMemberToken.Token = result.Token;
                    existingDeviceMemberToken.UpdatedDt = now;
                    db.MemberTokens.Update(existingDeviceMemberToken);
                }
                else
                {
                    var newMemberToken = new MemberToken()
                    {
                        MemberId = dbMember.MemberId,
                        Token = result.Token,
                        CreatedDt = now,
                        DeviceId = login.CrossSessionUniqueClientID
                    };
                    db.MemberTokens.Add(newMemberToken);
                }
                await db.SaveChangesAsync();


                Ok();
                return result;
            }
            else
            {
                result.Success = false;
                result.AccountLockedOut = false;

                try
                {
                    Uri? loginAttemptsUri = new Uri(baseUmbracoUrl + "/umbraco/api/MemberApi/LoginAttemptsCount");
                    using var LeftAttemptsResponse = await umbracoApiClient.PostAsync(loginAttemptsUri, postContent);
                    LeftAttemptsResponse.EnsureSuccessStatusCode();
                    var attempts = await LeftAttemptsResponse.Content.ReadAsStringAsync();
                    result.LeftLoginAttempts = int.Parse(attempts);
                    Console.WriteLine(result.LeftLoginAttempts);


                    uri = new Uri(baseUmbracoUrl + "/umbraco/api/MemberApi/IsLockedOut");
                    parameters.Remove("password");
                    postContent = new FormUrlEncodedContent(parameters);
                    using var response2 = await umbracoApiClient.PostAsync(uri, postContent);
                    response2.EnsureSuccessStatusCode(); //throw if httpcode is an error

                    var resultString2 = await response2.Content.ReadAsStringAsync();
                    if (resultString2.Equals("true"))
                    {
                        Ok();
                        result.AccountLockedOut = true;
                        return result;
                    }
                }
                catch (Exception accountDontExist)
                {
                    _logger.LogWarning("Account don't exist: " + login.Username);
                }
                Ok();
                return result;
            }

            BadRequest();
            return null;
        }

        [HttpPost("CreateFreeUser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<string> CraeteFreeUser([FromBody] CreateFreeMember freeMember)
        {
            HttpClient umbracoApiClient = new HttpClient();
            var uri = new Uri(baseUmbracoUrl + "/umbraco/api/MemberApi/CreateUserFromLounge");
            var parameters = new Dictionary<string, string>();
            parameters.Add("name", freeMember.Name);
            parameters.Add("password", freeMember.ClearTextPassword);
            parameters.Add("email", freeMember.Email);
            parameters.Add("phoneNumber", freeMember.PhoneNumber);
            if (!string.IsNullOrWhiteSpace(freeMember.Cvr))
            {
                parameters.Add("cvr", freeMember.Cvr);
            }
            var postContent = new FormUrlEncodedContent(parameters);
            HttpResponseMessage response = await umbracoApiClient.PostAsync(uri, postContent);
            response.EnsureSuccessStatusCode(); //throw if httpcode is an error
            var resultString = await response.Content.ReadAsStringAsync();

            string msgSales = "<h3>Hi Amanda,</h3> <h3>A new sign up as free user on the Lounge</h3>" +
                "<h4>Info:</h4>" +
                $" <h3>Name: <b>{freeMember.Name}</b>,</h3>" +
                $" <h3>Email: <b>{freeMember.Email}</b>,</h3>" +
                $" <h3>Phone: <b>{freeMember.PhoneNumber}</b>.</h3>";

            if (!string.IsNullOrWhiteSpace(freeMember.Cvr))
            {
                msgSales += $"<h3>CVR: <b>{freeMember.Cvr}</b>,</h3>";
            }

            msgSales += " <h4>Mvh</h4>" +
                "<h4> IT Department </h4>";
            //await EmailService.SendMailAsync("amza@startupcentral.dk", null, "New Free User Sign up " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), msgSales, null);
            return resultString;

        }

        [HttpGet("SearchMembers")]
        [ProducesResponseType(200)]
        public IEnumerable<MinimalMemberInfo> SearchMembers(string terms)
        {
            // See JS in BlazorServer const URL = 'https://api.startupcentral.dk/Members/SearchMembers?';
            //new ApiSession(this).Check(); //TODO: do some other kind of protection of this data. Maybe require the token as a parameter?
            var lowerTerms = terms.ToLowerInvariant();
            var lowerTerms_lenght = lowerTerms.Length;

            var result = cache_memberId2info.Values.Where(x => x.Name.Substring(0, Math.Min(x.Name.Length, lowerTerms_lenght)).ToLowerInvariant().StartsWith(lowerTerms));
            var membersInfo = result.OrderBy(x => x.Name).Select(x =>
                new MinimalMemberInfo { Id = x.Id, Name = x.Name });
            return membersInfo;
        }

        [HttpGet("ShouldShowNpsInput")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public bool ShouldShowNpsInput()
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            var memberId = apiSession.MyMemberId.Value;

            var firstLogin = db.ActiveMembers.Where(x => x.MemberId == memberId).OrderBy(x => x.LoginDate).FirstOrDefault()?.LoginDate;
            if (firstLogin == null)
                return false;
            if (DateTime.Now - firstLogin.Value < new TimeSpan(60, 0, 0, 0))
                return false;

            var lastNpsDt = db.MemberNetPromotorScores.Where(x => x.MemberId == memberId).OrderByDescending(x => x.ReportDate).FirstOrDefault()?.ReportDate;
            if (lastNpsDt == null)
                return true;
            if (DateTime.Now - lastNpsDt.Value > new TimeSpan(6 * 30, 0, 0, 0))
                return true;

            return false;
        }

        public class NpsInput
        {
            public byte NPS { get; set; }
            public String? Sugestion { get; set; }
        }

        [HttpGet("ReportNpsInput")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public void ReportNpsInput(NpsInput npsInput)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            var memberId = apiSession.MyMemberId.Value;

            db.MemberNetPromotorScores.Add(new MemberNetPromotorScore()
            {
                MemberId = memberId,
                ReportDate = DateTime.Now,
                Nps = npsInput.NPS,
                Sugestion = npsInput.Sugestion,
            });
            db.SaveChanges();
        }


        [HttpGet("test")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<string>> TestMApi()
        {
            return Ok("test passed");
        }

        [HttpGet("WeeklyActiveLoungeFreeMembers")]
        [ProducesResponseType(202)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<List<FreeActiveMembersData>> WeeklyActiveLoungeFreeMembers()
        {
            return await memberService.WeeklyActiveLoungeFreeMembers();
        }

        [HttpGet("MonthlyActiveLoungeFreeMembers")]
        [ProducesResponseType(202)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<List<FreeActiveMembersData>> MonthlyActiveLoungeFreeMembers()
        {
            return await memberService.MonthlyActiveLoungeFreeMembers();
        }

        [HttpGet("YearlyActiveLoungeFreeMembers")]
        [ProducesResponseType(202)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<List<FreeActiveMembersData>> YearlyActiveLoungeFreeMembers()
        {
            return await memberService.YearlyActiveLoungeFreeMembers();
        }


        [HttpPost("BlockMember")]
        [ProducesResponseType(202)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> BlockMember(int blockedMemberId)
        {
            try
            {
                var apiSession = new ApiSession(this);
                apiSession.Check();
                int browserId = apiSession.MyMemberId.Value;
                db.BlockedMembers.Add(new BlockedMember()
                {
                    MemberId = browserId, //apiSession.MyMemberId.Value,
                    BlockedMemberId = blockedMemberId,
                });
                await db.SaveChangesAsync();
                return Ok("Member Blocked");
            } catch(Exception e)
            {
                return StatusCode(500, e.Message);
                
            }
        }


        [HttpGet("ListOfBlockedMembers")]
        [ProducesResponseType(202)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<BlockedMember>>> BlockedMembers()
        {
            try
            {
                var apiSession = new ApiSession(this);
                apiSession.Check();
                int browserId = apiSession.MyMemberId.Value;
                var blockedMembers = await db.BlockedMembers.Where(x => x.MemberId == browserId).ToListAsync();
                return Ok(blockedMembers);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);

            }
        }

        [HttpPost("ReportUser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task ReportUser(MemberInfo reportedUser, string reason)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();

            var sb = new StringBuilder();
            sb.AppendLine($"Reported by: {cache_memberId2info[apiSession.MyMemberId.Value].Name} ({apiSession.MyMemberId.Value.ToString("D")})");
            sb.AppendLine("<br/>");
            sb.AppendLine("<br/>");
            sb.AppendLine($"User that was reported: {reportedUser.Name} ({reportedUser.Id})");
            sb.AppendLine("<br/>");
            sb.AppendLine($"Reason: {reason}");
            var body = sb.ToString();

            //await EmailService.SendMailAsync("vh@startupcentral.dk", null, $"A user was reported ({reportedUser.Id})", body, _logger);
        }

        private void AddMembersToDB()
        {
            var sw = Stopwatch.StartNew();
            lock (cache_memberId2info)
            {
                var allDbMembers = db.Members.ToList();

                var maxTicks = DateTime.MinValue.Ticks;
                if (allDbMembers.Any())
                {
                    maxTicks = allDbMembers.Max(x => x.UpdatedDt).Ticks;
                }

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                string jsonMembers = new WebClient().DownloadString(
                    baseUmbracoUrl +
                    // "http://localhost:1111" +
                    "/umbraco/api/Memberapi/GetLoungeMembers?maxTicks=" + maxTicks);
                var umbraco_members = JsonSerializer.Deserialize<List<UmbracoMemberInfo>>(jsonMembers);

                _logger.LogInformation("AddMembersToDB umbraco_members count: " + umbraco_members.Count);

                var db_id2member = allDbMembers.ToDictionary(x => x.MemberId, x => x);

                foreach (var umbracoMember in umbraco_members)
                {
                    try
                    {
                        Member existingDbMember;
                        db_id2member.TryGetValue(umbracoMember.Id, out existingDbMember);

                        var json = new MemberJson()
                        {
                            IsApproved = umbracoMember.IsApproved,
                            Alias = umbracoMember.ContentType,
                            Avatar = umbracoMember.Avatar,
                            IsAdmin = umbracoMember.IsAdmin,
                        };

                        var dbMemberFromUmbraco = new Member()
                        {
                            MemberId = umbracoMember.Id,
                            Name = umbracoMember.Name,
                            Email = umbracoMember.Email,
                            Login = umbracoMember.Login,
                            Url = string.IsNullOrEmpty(umbracoMember.Url) ? "" : umbracoMember.Url,
                            CreatedDt = umbracoMember.CreateDate,
                            // ensure that UpdatedDt is always >= CreatedDt ; which it already is
                            UpdatedDt = umbracoMember.UpdateDate < umbracoMember.CreateDate ? umbracoMember.CreateDate : umbracoMember.UpdateDate,
                            Password = string.IsNullOrEmpty(umbracoMember.RawPasswordValue) ? "" : umbracoMember.RawPasswordValue,
                            Json = JsonSerializer.Serialize(json)
                        };

                        if (existingDbMember != null)
                        {
                            if (existingDbMember.UpdatedDt < dbMemberFromUmbraco.UpdatedDt)
                            {
                                // https://stackoverflow.com/a/53575280 Note this will completely overwrite every property ...
                                db.Entry(existingDbMember).CurrentValues.SetValues(dbMemberFromUmbraco);
                                db.Members.Update(existingDbMember);
                            }
                        }
                        else
                        {
                            db.Members.Add(dbMemberFromUmbraco);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message + " id=" + umbracoMember.Id);
                    }
                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message + " on SaveChanges()");
                }
            }

            _logger.LogInformation("UpdateCache updatedDbMembers count: " + UpdateCache());

            _logger.LogInformation("AddMembersToDB finished in ms: " + sw.ElapsedMilliseconds);
        }

        internal static int UpdateCache()
        {
            lock (cache_memberId2info)
            {
                var fromDt = DateTimeOffset.MinValue;
                if (cache_memberId2info.Any())
                {
                    fromDt = cache_memberId2info.Max(x => x.Value.UpdateDate);
                }

                var db = new ScSoMeContext();
                var updatedDbMembers = db.Members.Where(x => x.UpdatedDt >= fromDt);

                foreach (var dbMember in updatedDbMembers)
                {
                    UmbracoMemberInfo um = new UmbracoMemberInfo();
                    var containsKey = cache_memberId2info.ContainsKey(dbMember.MemberId);
                    if (containsKey)
                        um = cache_memberId2info[dbMember.MemberId];
                    else
                        cache_memberId2info[dbMember.MemberId] = um;

                    um.Id = dbMember.MemberId;
                    um.Name = dbMember.Name;

                    um.CreateDate = dbMember.CreatedDt.LocalDateTime;
                    um.UpdateDate = dbMember.UpdatedDt.LocalDateTime;
                    um.Login = dbMember.Login;
                    um.Url = dbMember.Url;

                    if (!string.IsNullOrEmpty(dbMember.Json))
                    {
                        var memberJson = JsonSerializer.Deserialize<MemberJson>(dbMember.Json);
                        um.Avatar = memberJson.Avatar;
                        um.ContentType = memberJson.Alias;
                        um.IsApproved = memberJson.IsApproved;
                        um.IsAdmin = memberJson.IsAdmin;
                    }
                }

                return updatedDbMembers.Count();
            }
        }
    }
}
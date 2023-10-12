using Microsoft.EntityFrameworkCore;
using ScSoMe.API.Controllers.Members;
using ScSoMe.EF;

namespace ScSoMe.API.Services
{
    public class MemberService
    {
        private readonly ScSoMeContext db;

        private DateTime now = DateTime.Now;
        public MemberService()
        {
            db = new ScSoMeContext();
        }

        public void AddActiveMember(int memberId, DateTime time)
        {
            try
            {
                bool existsMember = db.ActiveMembers.Any(x => x.MemberId == memberId && x.LoginDate == time);
                if (!existsMember)
                {
                    var activeMember = new EF.ActiveMember()
                    {
                        MemberId = memberId,
                        LoginDate = time
                    };
                    db.ActiveMembers.Add(activeMember);
                    db.SaveChanges();
                }
                else
                    db.Update(existsMember);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public string DailyActiveMembers()
        {
            int activs = db.ActiveMembers.Where(x => x.LoginDate > now.AddHours(-24) && x.LoginDate <= now).Count();

            if (activs == 0)
                return "No active members in the past 24 hours";
            else
                return activs + " Active Members were found for the last 24 hours";

        }

        public string WeeklyActiveMembers()
        {
            int activs = db.ActiveMembers.Where(x => x.LoginDate > now.AddDays(-7) && x.LoginDate <= now).Count();

            if (activs == 0)
                return "No active members in the past 7 days";
            else
                return activs + " Active Members were found for the last 7 days";

        }



        public async Task<List<FreeActiveMembersData>> FreeActiveMembers(List<ActiveMember> notApprovedActives)
        {
            try
            {
                List<FreeActiveMembersData> freeActiveMembersDatas = new List<FreeActiveMembersData>();

                foreach (var um in notApprovedActives)
                {
                    var member = await db.Members.FirstOrDefaultAsync(x => x.MemberId == um.MemberId);
                    if (member != null)
                    {
                        var memberStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<UmbracoMemberInfo>(member.Json).IsApproved;
                        if (!memberStatus)
                        {
                            var exists = freeActiveMembersDatas.FirstOrDefault(x => x.MemberId == um.MemberId);

                            if (exists != null)
                            {
                                exists.LoginCount++;
                            }
                            else
                            {
                                if (member != null)
                                {
                                    freeActiveMembersDatas.Add(new FreeActiveMembersData
                                    {
                                        MemberId = um.MemberId,
                                        Name = member.Name,
                                        IsApproved = memberStatus,
                                        LoginCount = 1
                                    });

                                }
                            }
                        }

                    }
                }

                return freeActiveMembersDatas;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<List<FreeActiveMembersData>> WeeklyActiveLoungeFreeMembers()
        {
            var notApprovedActives = db.ActiveMembers.Where(x => x.LoginDate > now.AddDays(-7) && x.LoginDate <= now).ToList();
            return await FreeActiveMembers(notApprovedActives);

        }
        public async Task<List<FreeActiveMembersData>> MonthlyActiveLoungeFreeMembers()
        {
            var notApprovedActives = db.ActiveMembers.Where(x => x.LoginDate > now.AddMonths(-1) && x.LoginDate <= now).ToList();
            return await FreeActiveMembers(notApprovedActives);

        }
        public async Task<List<FreeActiveMembersData>> YearlyActiveLoungeFreeMembers()
        {
            var notApprovedActives = db.ActiveMembers.Where(x => x.LoginDate > now.AddYears(-1) && x.LoginDate <= now).ToList();
            return await FreeActiveMembers(notApprovedActives);

        }

    }

    public class FreeActiveMembersData
    {
        public int MemberId { get; set; }
        public string? Name { get; set; }
        public bool? IsApproved { get; set; }
        public int LoginCount { get; set; }
    }

}

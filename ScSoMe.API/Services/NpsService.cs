using Microsoft.EntityFrameworkCore;
using ScSoMe.EF;

namespace ScSoMe.API.Services
{
    public class NpsService
    {
        private ScSoMeContext _context;
        //private const string npsQuery = "with AM as (select MemberId, MinLoginDate = Min(LoginDate), Count(*) as LoginCount from [dbo].[ActiveMembers] group by MemberId)" +
        //    " SELECT AM.[MemberId]      ,[ReportDate]     ,[nps] ,sugestion  ,Name = (SELECT [name] FROM [dbo].[Members] M where AM.[MemberId] = M.member_id)  ,FirstLoginDays = DATEDIFF(DAY, AM.MinLoginDate, GETUTCDATE()) ,LoginCount  FROM [dbo].[MemberNetPromotorScore] N  right outer join AM on N.MemberId = AM.MemberId  where nps is not null order by FirstLoginDays desc";

        public NpsService()
        {
            _context = new ScSoMeContext();
        }

        public async Task<List<NpsResult>> RunNPS()
        {
            var results = new List<NpsResult>();
            var npsDb = await _context.MemberNetPromotorScores.OrderByDescending(x=> x.ReportDate).ToListAsync();
            foreach (var nps in npsDb)
            {
                var memberName = _context.Members.FirstOrDefault(x => x.MemberId == nps.MemberId);
                results.Add(new NpsResult { 
                
                  //  MemberId = nps.MemberId,
                    Name = memberName != null ? memberName.Name : "Error",
                    ReportDate = nps.ReportDate.ToString("MM/dd/yyyy hh:mm tt"),
                    Nps = nps.Nps,
                    Suggestion = nps.Sugestion != null ? nps.Sugestion : "No sugestions",

                });
            }

            //var queryResults = await _context.MemberNetPromotorScores.FromSqlRaw(npsQuery).ToListAsync();

            return results;
        }
    }

    public class NpsResult
    {
       // public int MemberId { get; set; }
        public string? Name { get; set; }
        public string? ReportDate { get; set; }
        public byte Nps { get; set; }
        public string? Suggestion { get; set; }

    }

}

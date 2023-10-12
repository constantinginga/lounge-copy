using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ScSoMe.API.Controllers.Members.MembersController;

namespace ScSoMe.API
{
    public class ApiSession
    {
        private readonly ControllerBase ctrl;        

        public ApiSession(ControllerBase ctrl)
        {
            this.ctrl = ctrl;
        }

        public void Check()
        {
            if (!MembersController.cache_memberId2info.Any()) MembersController.UpdateCache();            

            if (MyMemberId == null)
            {
                ctrl.Response.StatusCode = 501;
                throw new Exception("Missing member session - Login");
            }
        }

        internal int? MyMemberId
        {
            get
            {
                var tokenHeaderValues = ctrl.Request.Headers["X-ScSoMe-Token"];
                if (tokenHeaderValues != Microsoft.Extensions.Primitives.StringValues.Empty)
                {
                    var token = tokenHeaderValues.Single();
                    var memberId = MembersController.cache_token2memberId.GetValueOrDefault(token);
                    if (memberId == 0)
                    {
                        var db = new ScSoMe.EF.ScSoMeContext();
                        var existingToken = db.MemberTokens.SingleOrDefault(x => x.Token == token);
                        if (existingToken != null)
                        {
                            memberId = existingToken.MemberId;
                            MembersController.cache_token2memberId.TryAdd(token, memberId);
                        }
                    }

                    if (memberId != 0) 
                        return memberId;
                }
                return null;
            }
        }

    }
}

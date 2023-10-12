using Microsoft.AspNetCore.Mvc;
using ScSoMe.EF;

namespace ScSoMe.API.Controllers.Groups.GroupsController
{
    [ApiController]
    [Route("[controller]")]
    public class GroupsController : ControllerBase
    {

        private readonly ILogger<GroupsController> _logger;
        private readonly ScSoMeContext db;

        public GroupsController(ILogger<GroupsController> logger)
        {
            _logger = logger;
            db = new ScSoMeContext();
        }

        [HttpGet("GetGroups")]
        public IEnumerable<ApiDtos.ScGroup>? GetGroups()
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();

            try
            {
                var dbGroups = db.Groups.Select(x => x);

                var result = dbGroups.Select(x => new ApiDtos.ScGroup()
                {
                    GroupId = x.GroupId,
                    GroupName = x.Groupname,
                    Url = x.Url,
                    CreatedDt = x.CreatedDt
                });

                if (result.Count() == 0)
                {
                    // default to create at least one group
                    var now = DateTimeOffset.Now;
                    db.Groups.Add(new Group()
                    {
                        GroupId = 1,
                        Groupname = "Lounge",
                        Url = "Lounge",
                        CreatedDt = now,
                        UpdatedDt = now,
                    });
                    db.SaveChanges();
                    return GetGroups();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                Response.StatusCode = 500;
                return null;
            }
        }



        [HttpGet("MarkAsReadUntilNow")]
        public void MarkAsReadUntilNow(int groupId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            var memberId = apiSession.MyMemberId;

            var groupsRead = db.GroupsReads.SingleOrDefault(x => x.MemberId == memberId && x.GroupId == groupId);
            if (groupsRead != null)
            {
                groupsRead.LastReadDt = DateTime.Now;
                db.SaveChanges();
            }
        }
        

        [HttpGet("NumberOfNewPosts")]
        public TrackedAndNewPosts NumberOfNewPosts(int groupId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            var memberId = apiSession.MyMemberId;

            TrackedAndNewPosts result = new TrackedAndNewPosts();
            var groupsRead = db.GroupsReads.SingleOrDefault(x => x.MemberId == memberId && x.GroupId == groupId);
            if (groupsRead == null)
            {
                var lastLogin = db.ActiveMembers.OrderByDescending(x => x.LoginDate).Skip(1).FirstOrDefault(x => x.MemberId == memberId)?.LoginDate;
                if (!lastLogin.HasValue)
                    lastLogin = DateTime.Now.AddDays(-180);

                var newGR = new GroupsRead() { GroupId = groupId, MemberId = memberId.Value, LastReadDt = lastLogin.Value, NotifyOnNew = false };
                db.GroupsReads.Add(newGR);

                try
                {
                    db.SaveChanges();
                }
                catch (Exception errDuplicateKey)
                {
                    _logger.LogTrace("Duplicate key? " + errDuplicateKey);
                }
                groupsRead = db.GroupsReads.Single(x => x.MemberId == memberId && x.GroupId == groupId);
            }

            var newPosts = db.Comments
                .Where(x => x.GroupId == groupId)
                .Where(x => x.RootCommentId == x.CommentId) // ignore comments select only posts
                .Where(x => x.CreatedDt > groupsRead.LastReadDt)
                .Count();

            result.NewPosts = newPosts;
            result.Tracked = groupsRead.NotifyOnNew;

            return result;
        }

        [HttpPost("GroupTracked")]
        [ProducesResponseType(201)]
        [ProducesResponseType(500)]
        public async Task GroupTracked(int groupId, bool tracked)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            int memberId = apiSession.MyMemberId.Value;

            try
            {
                var gr = await db.GroupsReads.FindAsync(memberId, groupId);
                if (gr == null) throw new Exception("GroupsRead not found for memberId:" + memberId + " groupId:" + groupId);
                gr.NotifyOnNew = tracked;
                db.Update(gr);
                await db.SaveChangesAsync();
                Ok();
                Response.StatusCode = 201;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                Response.StatusCode = 500;
            }
        }
        [HttpPost("CreateGroup")]
        [ProducesResponseType(201)]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task CreateGroup(string groupName, string groupURL)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();


            await db.Groups.AddAsync(new Group { GroupId = GetNextGroupId(), Groupname = groupName, Url = groupURL, CreatedDt = DateTime.Now, UpdatedDt = DateTime.Now });
            await db.SaveChangesAsync();
            Ok();
        }

        [HttpDelete("RemoveGroup")]
        [ProducesResponseType(201)]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task RemoveGroup(string groupName)
        {
            //var apiSession = new ApiSession(this);
            //apiSession.Check();

            if (groupName != null)
            {
                var foundGroup = db.Groups.Where(gr => gr.Groupname.Equals(groupName.ToLower())).FirstOrDefault();

                if (foundGroup != null)
                {
                    var g = db.Groups.Remove(foundGroup).Entity;
                    await db.SaveChangesAsync();
                    if (g != null)
                    {
                        Ok();
                    }
                }
            }
        }

        private static int lastGroupId = -1;
        private static object lastGroupIdLockObject = new object();

        private int GetNextGroupId()
        {
            if (lastGroupId == -1)
            {
                lock (lastGroupIdLockObject)
                {
                    if (lastGroupId == -1)
                    {
                        if (null == db.Groups.FirstOrDefault())
                        {
                            return ++lastGroupId;
                        }
                        lastGroupId = db.Groups.Max(x => x.GroupId);
                        return ++lastGroupId;
                    }
                }
            }
            lock (lastGroupIdLockObject)
            {
                return ++lastGroupId;
            }
        }
    }
}
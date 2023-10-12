using Microsoft.AspNetCore.Mvc;
using ScSoMe.API.Services;
using ScSoMe.ApiDtos;
using EF6 = ScSoMe.EF;
using System.Text.Json;
using ScSoMe.API.Controllers.Members.MembersController;
using HtmlAgilityPack;
using System.Text;
using System.Collections;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;

namespace ScSoMe.API.Controllers.Comments.CommentsController
{

    [ApiController]
    [Route("[controller]")]
    public class CommentsController : ControllerBase
    {
        //ATTRIBUTES
        private readonly ILogger<CommentsController> _logger;
        private readonly EF6.ScSoMeContext db;
        private readonly TrackingMessageService trackingService;
        private readonly NotificationsService notificationsService;
        private static long lastMsgId = -1;
        private static object lastMsgIdLockObject = new object();
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<long, object> likeLocks = new System.Collections.Concurrent.ConcurrentDictionary<long, object>();
        private static bool clearingLikeLocks = false;
        private HttpClient client;
        private readonly TranslationService translationService;
        private readonly string _azureConnectionString;
        private readonly string _azureContainerName;

        public CommentsController(ILogger<CommentsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            db = new EF6.ScSoMeContext();
            trackingService = new TrackingMessageService();
            notificationsService = new NotificationsService();
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Basic d290aWc5ODkwN0BkdWZlZWQuY29tOlcwUTl6eE5peE9hRlhUN3VSRG9X");
            translationService = new TranslationService();
            _azureConnectionString = configuration.GetConnectionString("AzureConnectionString");
            _azureContainerName = "upload-container";
        }

        private long GetNextMsgId()
        {
            if (lastMsgId == -1)
            {
                lock (lastMsgIdLockObject)
                {
                    if (lastMsgId == -1)
                    {
                        if (null == db.Comments.FirstOrDefault())
                        {
                            return ++lastMsgId;
                        }
                        lastMsgId = db.Comments.Max(x => x.CommentId);
                        return ++lastMsgId;
                    }
                }
            }
            lock (lastMsgIdLockObject)
            {
                return ++lastMsgId;
            }
        }

        [HttpPost("SearchForText")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IList<MatchingPost>> SearchForText(string like, int? groupId = null)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();

            // from https://gunnarpeipman.com/ef-core-like-operator/
            var matches = from p in db.Comments
                          orderby p.CommentId descending
                          where Microsoft.EntityFrameworkCore.EF.Functions.Like(p.Text, "%" + like + "%")
                          select p;

            if (groupId.HasValue)
            {
                matches = matches.Where(g => g.GroupId == groupId.Value);
            }

            var matchList = new List<MatchingPost>();
            foreach (var match in matches)
            {
                matchList.Add(new MatchingPost()
                {
                    PostId = match.RootCommentId.Value,
                    MessageId = match.CommentId,
                    GroupId = match.GroupId, 
                    DateTime = new DateTime(Math.Max(match.CreatedDt.Ticks, match.UpdatedDt.Ticks))
                });
            }
            return matchList;
        }


        public class OpenGraphInfo
        {
            public string Title { get; set; }
            public long UnixTime { get; set; }
            public string Description { get; set; }
            public string Image { get; internal set; }
        }

        [HttpPost("GetOpenGraphForPost")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<OpenGraphInfo> GetOpenGraphForPost(long postId)
        {
            var msg = db.Comments.SingleOrDefault(x => x.CommentId == postId);
            if (msg == null || msg.RootCommentId != msg.CommentId)
            {
                Response.StatusCode = 404;
                return null;
            }

            if (!MembersController.cache_memberId2info.Any()) MembersController.UpdateCache();
            var memberName = MembersController.cache_memberId2info.GetValueOrDefault(msg.AuthorMemberId).Name;
            var groupName = (await db.Groups.FindAsync(msg.GroupId)).Groupname;

            // https://stackoverflow.com/questions/2113651/how-to-extract-text-from-resonably-sane-html
            //var text = new FastHtmlTextExtractor().Extract(msg.Text.ToCharArray(), true); // not working as expected
            var text = "";

            var extractedSampleText = new StringBuilder();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(msg.Text);
            if (doc != null && doc.DocumentNode != null)
            {
                foreach (var script in doc.DocumentNode.Descendants("script").ToArray())
                {
                    script.Remove();
                }
                foreach (var style in doc.DocumentNode.Descendants("style").ToArray())
                {
                    style.Remove();
                }
                var allTextNodes = doc.DocumentNode.SelectNodes("//text()");
                if (allTextNodes != null && allTextNodes.Count > 0)
                {
                    foreach (HtmlNode node in allTextNodes)
                    {
                        extractedSampleText.Append(node.InnerText);
                        extractedSampleText.Append(" ");
                    }
                }
                text = extractedSampleText.ToString().Replace("  ", " ");
            }
            var maxLength = Math.Min(text.Count(), 350);
            text = text.Substring(0, maxLength);
            if (maxLength == 350) text += "...";

            string imgUrl = "";
            if (!string.IsNullOrEmpty(msg.EmbeddedUrl))
            {
                imgUrl = JsonSerializer.Deserialize<ApiDtos.Embedded>(msg.EmbeddedUrl).Img;
            }

            var result = new OpenGraphInfo()
            {
                Title = memberName + " posted in " + groupName + " on " + msg.CreatedDt.ToString("yyyy-MM-dd HH:mm"),
                UnixTime = (long)msg.UpdatedDt.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
                Description = text,
                Image = imgUrl
            };

            return result;
        }

        [HttpPost("ReportMessageId")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task ReportMessageId(WriteMessage msgIdAndReportText)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();

            var sb = new StringBuilder();
            sb.AppendLine("Reposted by MemberId : ");
            sb.AppendLine(apiSession.MyMemberId.Value.ToString("D"));
            sb.AppendLine("<br/>Name : ");
            sb.AppendLine(MembersController.cache_memberId2info[apiSession.MyMemberId.Value].Name);
            sb.AppendLine("<br/>");
            sb.AppendLine("<br/>");
            sb.AppendLine("Report text on comment_id=");
            sb.AppendLine(msgIdAndReportText.Id.ToString("D"));
            sb.AppendLine(" : <br/><div>");
            sb.AppendLine(msgIdAndReportText.Text);
            sb.AppendLine("</div><br/><br/>");

            var msg = db.Comments.Single(x => x.CommentId == msgIdAndReportText.Id);
            var postId = msg.RootCommentId.Value;
            var postMessage = db.Comments.Single(x => x.CommentId == postId);
            var group = db.Groups.Single(x => x.GroupId == postMessage.GroupId);
            var url = "https://www.startupcentral.dk/Lounge/groups/" + group.Url + "/" + postId.ToString("D");
            sb.Append("<a href=\"");
            sb.Append(url);
            sb.Append("\">");
            sb.Append(group.Groupname);
            sb.Append(" - ");
            sb.Append(postId);
            sb.AppendLine("</a> : <br/>");

            sb.AppendLine("<i>Created ");
            sb.AppendLine(msg.CreatedDt.ToString("yyyy-MM-dd HH:mm"));
            sb.AppendLine(" by ");
            sb.AppendLine(MembersController.cache_memberId2info[msg.AuthorMemberId].Name);
            sb.AppendLine("<br/>Updated ");
            sb.AppendLine(msg.UpdatedDt.ToString("yyyy-MM-dd HH:mm"));
            sb.AppendLine("</i><div>");
            sb.AppendLine(msg.Text);
            sb.AppendLine("</div>");

            var body = sb.ToString();

            //await EmailService.SendMailAsync("vh@startupcentral.dk", null, "Report-User-Content: " + msgIdAndReportText.Id, body, _logger);
        }

        [HttpPost("EditText")]
        [ProducesResponseType(202)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task EditText(WriteMessage idAndNewText)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();

           
            // assume same AuthorMemberId
            try
            {
                var dbMessage = await db.Comments.FindAsync(idAndNewText.Id);
                if (dbMessage == null)
                {
                    NotFound();
                    return;
                }
                else
                {
                    if (dbMessage.AuthorMemberId == apiSession.MyMemberId
                        // Let admins delete+edit
                        || MembersController.cache_memberId2info[apiSession.MyMemberId.Value].IsAdmin)
                    {
                        dbMessage.Text = idAndNewText.Text;
                        dbMessage.UpdatedDt = DateTimeOffset.Now;

                        await HandleMentions(idAndNewText, dbMessage.RootCommentId.Value, apiSession.MyMemberId.Value, dbMessage.UpdatedDt, dbMessage.GroupId);

                        await db.SaveChangesAsync();
                        Ok();
                        Response.StatusCode = 202;
                        return;
                    }
                    else
                    {
                        throw new Exception("Only the original author or and Admin can modify the text");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "On EditText messageId:{messageId}", idAndNewText?.Id);
                if (Response.StatusCode == 0) Response.StatusCode = 500;
                return;
            }
        }

        private async Task<bool> HandleMentions(WriteMessage idAndNewText, long rootCommentId, int byMemberId, DateTimeOffset time, int groupId)
        {
            try
            {
                var searchString = "\" data-denotation-char=\"\" data-id=\"";
                var searchIndex = 0;
                var hitIndeices = new List<int>();
                var hitIndex = -1;
                do
                {
                    hitIndex = idAndNewText.Text.IndexOf(searchString, searchIndex);
                    if (hitIndex > 0)
                    {
                        hitIndex = hitIndex + searchString.Length;
                        hitIndeices.Add(hitIndex);
                        searchIndex = hitIndex;
                    }
                } while (hitIndex > 0);

                if (!hitIndeices.Any())
                    return false;

                _logger.LogInformation("Found mentions in messageId: " + idAndNewText.Id);

                var mentionedIds = new List<int>();
                foreach (var index in hitIndeices)
                {
                    var idString = idAndNewText.Text.Substring(index, 10);
                    var idStringLength = idString.IndexOf('"', 1);
                    idString = idString.Substring(0, idStringLength);
                    mentionedIds.Add(int.Parse(idString));
                }

                var notificationMessage = "";
                if (rootCommentId == idAndNewText.Id)
                {
                    notificationMessage = "A post by ";
                }
                else
                {
                    notificationMessage += "A comment by ";
                }
                notificationMessage += MembersController.cache_memberId2info[byMemberId].Name
                    + " dated " + time.ToString("yyyy-MM-dd HH:mm")
                    + " mentions you: <br/>" + Environment.NewLine;
                notificationMessage += idAndNewText.Text;

                var groupUrl = new EF6.ScSoMeContext().Groups.Single(x => x.GroupId == groupId).Url;
                var url = "https://www.startupcentral.dk/Lounge/groups/" + groupUrl + "/" + rootCommentId.ToString("D");

                notificationMessage += "<br/>" + url;

                var bccs = new List<string>();
                foreach (var memberId in mentionedIds)
                {
                    bccs.Add(MembersController.cache_memberId2info[memberId].Login);
                    await trackingService.SetTracked(rootCommentId, memberId, false, false, false, false, true, byMemberId);
                }

                //done via the EmailNotificationsService instead of here:
                //EmailService.SendMailAsync("", bccs, "Startup Lounge | Mentions you", notificationMessage, _logger);

                return true;
            }
            catch (Exception)
            {
                return false;
                // ignore
            }

        }

        [HttpPost("CreatePost")]
        [ProducesResponseType(201)]
        [ProducesResponseType(500)]
        public async Task<long> CreatePost(int belongsToGroupId, bool hasMedia,WriteMessage post)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            var authorId = apiSession.MyMemberId.Value;
            try
            {
                var now = DateTimeOffset.Now;
                var postId = GetNextMsgId();
                var dbPost = new EF.Comment()
                {
                    CommentId = postId,
                    GroupId = belongsToGroupId,
                    ParentCommentId = null,
                    RootCommentId = postId,
                    Text = post.Text,
                    AuthorMemberId = authorId,
                    CreatedDt = now,
                    UpdatedDt = now,
                    LikersJson = null,
                    HasMedia= hasMedia
                };
                db.Comments.Add(dbPost);
                db.SaveChanges();
                Response.StatusCode = 201;

                // do in parallel
                var handleMentionsTask = HandleMentions(post, postId, authorId, dbPost.CreatedDt, belongsToGroupId);
                var setTrackedTask = trackingService.SetTracked(postId, authorId, false, false, false, true, false, 0);
                var setNewPostNotifTask = notificationsService.SetNewPostNotificationAsync(belongsToGroupId, postId, authorId);
                // actually it may not be necessary to wait for their completion
                Task.WaitAll(handleMentionsTask, setNewPostNotifTask, setNewPostNotifTask);

                return dbPost.CommentId;
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return 0;
            }
        }


        [HttpPost("CreateComment")]
        [ProducesResponseType(201)]
        [ProducesResponseType(500)]
        public async Task<long> CreateComment(long parentMessageId, long belongsToPostId, int belongsToGroupId, WriteMessage comment)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            try
            {
                var now = DateTimeOffset.Now;
                var nextMsgId = GetNextMsgId();
                var dbPost = new EF.Comment()
                {
                    CommentId = nextMsgId,
                    GroupId = belongsToGroupId,
                    ParentCommentId = parentMessageId,
                    RootCommentId = belongsToPostId,
                    Text = comment.Text,
                    AuthorMemberId = apiSession.MyMemberId.Value,
                    CreatedDt = now,
                    UpdatedDt = now,
                    LikersJson = null
                };
                db.Comments.Add(dbPost);
                db.SaveChanges();
                Response.StatusCode = 201;

                await trackingService.SetTracked(belongsToPostId, apiSession.MyMemberId.Value, false, false, true, false, false, 0, nextMsgId);

                await HandleMentions(comment, belongsToPostId, apiSession.MyMemberId.Value, dbPost.CreatedDt, belongsToGroupId);




                return dbPost.CommentId;
            }
            catch (Exception)
            {
                Response.StatusCode = 500;
                return 0;
            }
        }


        [HttpGet("GetLikers")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public List<MemberLike> GetLikers(long messageId)
        {
            new ApiSession(this).Check();
            var dbMsg = db.Comments.FirstOrDefault(x => x.CommentId == messageId);
            if (dbMsg == null) throw new Exception("Message with messageId not found: " + messageId);

            List<Like> likers = null;
            if (!string.IsNullOrWhiteSpace(dbMsg.LikersJson))
            {
                likers = JsonSerializer.Deserialize<List<Like>>(dbMsg.LikersJson);
            }
            else likers = new List<Like>();

            var result = new List<MemberLike>(likers.Count);

            foreach (var like in likers)
            {
                if (like.MemberId != 0)
                {
                    var um = MembersController.cache_memberId2info[like.MemberId];
                    var ml = new MemberLike()
                    {
                        LikeType = like.LikeType,
                        MemberId = like.MemberId,
                        MemberName = um.Name
                    };
                    result.Add(ml);
                }
            }
            return result;
        }



        /// <summary>
        /// Like or UnLike a post or comment
        /// </summary>
        /// <param name="likeCommand">Has the necessary input parameters</param>
        /// <returns>True if liked and false if unliked</returns>
        [HttpPost("LikeMsg")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public bool LikeMsg([FromBody] LikeCommand likeCommand)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            //TrackingMessageService service = new TrackingMessageService();
            int browserId = apiSession.MyMemberId.Value;
            long messageId = likeCommand.MessageId;
            int likeType = likeCommand.LikeType;
            bool result;

            try
            {
                if (clearingLikeLocks)
                {
                    lock (likeLocks)
                    {
                    }
                }

                var lockObject = likeLocks.GetOrAdd(messageId, new object());
                lock (lockObject)
                {
                    var dbMsg = db.Comments.FirstOrDefault(x => x.CommentId == messageId);
                    if (dbMsg == null) throw new Exception("Message with messageId not found");

                    List<Like> likers = null;
                    if (!string.IsNullOrWhiteSpace(dbMsg.LikersJson))
                    {
                        likers = JsonSerializer.Deserialize<List<Like>>(dbMsg.LikersJson);
                        // only to handle breaking serialization change in ScSoMe\ScSoMe.ApiDtos\Like.cs :
                        // likers = likers.Where(x => x.MemberId != 0).ToList();
                    }
                    else likers = new List<Like>();

                    var found = false;
                    var index = -1;
                    foreach (var like in likers)
                    {
                        index++;
                        if (like.MemberId == browserId)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        // UnLike
                        likers.RemoveAt(index);
                        dbMsg.LikersJson = JsonSerializer.Serialize(likers);
                        db.Comments.Update(dbMsg);
                        db.SaveChanges();
                        result = false;
                        Ok();
                    }
                    else
                    {
                        likers.Add(new Like() { MemberId = browserId, LikeType = likeType });
                        dbMsg.LikersJson = JsonSerializer.Serialize(likers);
                        db.Comments.Update(dbMsg);
                        db.SaveChanges();

                        if (dbMsg.RootCommentId != null)
                        {
                            long root = dbMsg.RootCommentId.Value;
                            trackingService.SetTracked(root, browserId, false, true, false, false, false, 0);
                        }
                        else
                            trackingService.SetTracked(messageId, browserId, false, true, false, false, false, 0);

                        result = true;
                        Ok();
                    }
                }

                const int maxLocks = 200;
                if (likeLocks.Count > maxLocks)
                {
                    lock (likeLocks)
                    {
                        if (likeLocks.Count > maxLocks)
                        {
                            clearingLikeLocks = true;
                            likeLocks.Clear();
                            clearingLikeLocks = false;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "On LikeMsg browserId:{browserId} messageId:{messageId} likeType:{likeType}", browserId, messageId, likeType);
                Response.StatusCode = 500;
                return false;
            }
        }


        //track and untrack posts manually 
        [HttpPost("TrackMsg")]
        [ProducesResponseType(201)]
        [ProducesResponseType(500)]
        public async Task TrackMsg([FromBody] TrackCommand trackCommand)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            //TrackingMessageService service = new TrackingMessageService();
            int browserId = apiSession.MyMemberId.Value;
            long messageId = trackCommand.PostId;

            try
            {
                var dbMsg = db.Comments.FirstOrDefault(x => x.CommentId == messageId);
                if (dbMsg == null) throw new Exception("Message with messageId not found");

                await trackingService.SetTracked(messageId, browserId, true, false, false, false, false, 0);
                Ok();
                Response.StatusCode = 201;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "On TrackMsg browserId:{browserId} messageId:{messageId}", browserId, messageId);
                Response.StatusCode = 500;
            }
        }

        [HttpPost("UnTrackMsg")]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public void UnTrackMsg([FromBody] TrackCommand trackCommand)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            //TrackingMessageService service = new TrackingMessageService();
            int browserId = apiSession.MyMemberId.Value;
            long messageId = trackCommand.PostId;

            try
            {
                var dbMsg = db.Comments.FirstOrDefault(x => x.CommentId == messageId);
                if (dbMsg == null) throw new Exception("Message with messageId not found");

                trackingService.RemoveTrackingInfo(messageId, browserId);
                Ok();
                Response.StatusCode = 204;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "On UnTrackMsg browserId:{browserId} messageId:{messageId}", browserId, messageId);
                Response.StatusCode = 500;
            }
        }

        [HttpGet("GetPostSeen")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<int> GetPostSeen(long postId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();

            return db.PostFirstReads.Count(x=>x.RootCommentId == postId);
        }

        [HttpGet("SetPostSeen")]
        [ProducesResponseType(200)]        
        [ProducesResponseType(500)]
        public async Task SetPostSeen(long postId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            int browserId = apiSession.MyMemberId.Value;
            var existingRow = await db.PostFirstReads.FindAsync(postId, browserId);
            if (existingRow == null)
            {
                await db.PostFirstReads.AddAsync(new EF6.PostFirstRead() { RootCommentId = postId, MemberId = browserId, FirstDt = DateTime.Now });
                await db.SaveChangesAsync();
            }
        }


        [HttpGet("PostIsTracked")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public bool PostIsTracked(long postId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            int browserId = apiSession.MyMemberId.Value;
            var trackedPosts = db.TrackedMessages.Where(x => x.MemberId == browserId && x.PostId == postId);
            Ok();
            if (trackedPosts.Any()) return true;
            return false;
        }

        /*
        [HttpGet("GetAllTrackedPosts")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public List<ApiDtos.TrackedMessage>? GetAllTrackedPosts()
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            // int browserId = apiSession.MyMemberId.Value;

            var trackedPosts = db.TrackedMessages.ToList();
            if (trackedPosts.Count > 0)
            {
                var results = new List<ApiDtos.TrackedMessage>();
                foreach (var trackedMessage in trackedPosts)
                {
                    var trackedDto = new ApiDtos.TrackedMessage()
                    {
                        MemberId = trackedMessage.MemberId,
                        PostId = trackedMessage.PostId,
                        Liked = trackedMessage.Liked,
                        Commented = trackedMessage.Commented,
                        IsPostCreator = trackedMessage.IsPostCreator,
                        Mentioned = trackedMessage.Mentioned,
                        UpdatedDt = trackedMessage.UpdatedDt,
                        InsertedDt = trackedMessage.InsertedDt
                    };
                    results.Add(trackedDto);
                }
                return results;
            }
            else
            {
                return null;
            }
        }

        [HttpGet("GetMemberTrackedPosts")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public List<ApiDtos.TrackedMessage>? GetMemberTrackedPosts()
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            int browserId = apiSession.MyMemberId.Value;

            var trackedPosts = db.TrackedMessages.Where(x => x.MemberId == browserId).ToList();
            if (trackedPosts.Count > 0)
            {
                var results = new List<ApiDtos.TrackedMessage>();
                foreach (var trackedMessage in trackedPosts)
                {
                    var trackedDto = new ApiDtos.TrackedMessage()
                    {
                        MemberId = trackedMessage.MemberId,
                        PostId = trackedMessage.PostId,
                        Liked = trackedMessage.Liked,
                        Commented = trackedMessage.Commented,
                        IsPostCreator = trackedMessage.IsPostCreator,
                        Mentioned = trackedMessage.Mentioned,
                        UpdatedDt = trackedMessage.UpdatedDt,
                        InsertedDt = trackedMessage.InsertedDt
                    };
                    results.Add(trackedDto);
                }
                return results;
            }
            else
            {
                return null;
            }
        }
        */

        [HttpGet("DeleteMessageAndAllChildren")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task DeleteMessageAndAllChildren(long messageId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();

            var dbMessage = db.Comments.SingleOrDefault(c => c.CommentId == messageId);

            if (dbMessage == null)
            {
                NotFound();
                Response.StatusCode = 404;
                return;
            }

            var userActionValid = dbMessage.AuthorMemberId == apiSession.MyMemberId
                // Let admins delete+edit
                || MembersController.cache_memberId2info[apiSession.MyMemberId.Value].IsAdmin;
            if (!userActionValid)
            {
                throw new Exception("Only the original author:" + dbMessage.AuthorMemberId + " or an Admin can delete the target message:" + messageId);
            }

            var dbChildMessages = db.Comments.Where(x => x.GroupId == dbMessage.GroupId && x.ParentCommentId == dbMessage.CommentId).ToList();
            if (dbChildMessages.Any())
                DeleteComments(dbChildMessages);

            db.Comments.Remove(dbMessage);
            if (messageId == dbMessage.RootCommentId.Value)
            {
                await trackingService.RemoveMsgTrackInfo(messageId);
                await notificationsService.RemoveNotifications(messageId);
                await translationService.DeleteTranslation(messageId);
            }
            db.SaveChanges();

            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/DELETE#responses
            Response.StatusCode = 204;
            return;
        }

        private void DeleteComments(List<EF.Comment> dbMessages)
        {
            foreach (var dbMessage in dbMessages)
            {
                var dbChildMessages = db.Comments.Where(x => x.GroupId == dbMessage.GroupId && x.ParentCommentId == dbMessage.CommentId).ToList();
                if (dbChildMessages.Any())
                    DeleteComments(dbChildMessages);

                db.Comments.Remove(dbMessage);
            }
        }

        [HttpGet("GetCommentWithoutChildren")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public ApiDtos.Comment? GetCommentWithoutChildren(long messageId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            int browserId = apiSession.MyMemberId.Value;
            try
            {
                var dbComment = db.Comments.SingleOrDefault(c => c.CommentId == messageId);

                if (dbComment == null)
                {
                    Response.StatusCode = 404;
                    NotFound();
                    return null;
                }

                var dtoComment = new Post();
                HandleMsgRead(dbComment, dtoComment, browserId);
                dtoComment.Responses = new List<ApiDtos.Comment>();
                Response.StatusCode = 200;
                return dtoComment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "On GetCommentWithoutChildren: browserId:{browserId} messageId:{messageId}", browserId, messageId);
                Response.StatusCode = 500;
                return null;
            }
        }


        [HttpGet("GetPostWithComments")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public ActionResult<Post?> GetPostWithComments(long postId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            int browserId = apiSession.MyMemberId.Value;
            try
            {
                var postAndAllComments = db.Comments.Where(
                    // TODO: Every c has RootCommentId so remove this line:
                    c => // c.CommentId == postId ||
                    c.RootCommentId == postId)
                    .OrderByDescending(x => x.UpdatedDt)
                    .ToList();

                if (postAndAllComments.Count == 0)
                {
                    Response.StatusCode = 404;
                    return new NotFoundResult();
                }

                var dbPost = postAndAllComments.Single(c => c.CommentId == postId);

                var dtoPost = new Post();
                dtoPost.GroupId = dbPost.GroupId;
                HandleMsgRead(dbPost, dtoPost, browserId);

                var dbAllPostComments = postAndAllComments.Where(c =>
                        c.RootCommentId == dbPost.CommentId
                        && c.ParentCommentId != null) // Only comments not the post
                    .ToLookup(x => x.ParentCommentId, x => x);

                dtoPost.Responses = HandleCommentsRead(dbPost.CommentId, dbAllPostComments, browserId);

                Response.StatusCode = 200;
                return dtoPost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "On GetPostWithComments: browserId:{browserId} postId:{postId}", browserId, postId);
                Response.StatusCode = 500;
                return null;
            }
        }

        [HttpGet("GetLatestLimitedPostsForGroup")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public IList<Post>? GetLatestLimitedPostsForGroup(int groupId, DateTimeOffset? fromDate = null, int takeLimit = 1, bool includeAllComments = true)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            int browserId = apiSession.MyMemberId.Value;
            try
            {
                if (fromDate == null)
                {
                    fromDate = DateTimeOffset.Now;
                }

                var groupPosts = db.Comments.Where(c =>
                    c.GroupId == groupId && c.UpdatedDt <= fromDate
                    && c.ParentCommentId == null) // Only posts no comments
                    .OrderByDescending(x => x.UpdatedDt)
                    .Take(takeLimit)
                    .ToList();

                return GetPostDetails(includeAllComments, browserId, groupPosts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "On GetLatestLimitedPostsForGroup: browserId:{browserId} groupId:{groupId} fromDate:{fromDate} takeLimit:{takeLimit} includeAllComments:{includeAllComments}",
                    browserId, groupId, fromDate, takeLimit, includeAllComments);
                Response.StatusCode = 500;
                return null;
            }
        }

        // How to use attributes and comments https://github.com/domaindrivendev/Swashbuckle.AspNetCore#include-descriptions-from-xml-comments
        /// <summary>
        /// Get the posts for a specific group from the viewpoint of a specific member
        /// </summary>        
        /// <param name="groupId">The group to get posts from</param>
        /// <param name="fromDate">From a specific date and back in time. Defaults to Now.</param>
        /// <param name="daysBack">A specific number of days back in time. Defaults to 30 days.</param>
        /// <param name="includeAllComments">Include Responses to Post and Responses to Responses</param>
        /// <returns>Posts including comments</returns>
        [HttpGet("GetLatestPostsForGroup")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public IList<Post>? GetLatestPostsForGroup(int groupId, DateTimeOffset? fromDate = null, int daysBack = 30, bool includeAllComments = true)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            int browserId = apiSession.MyMemberId.Value;
            try
            {
                if (fromDate == null)
                {
                    fromDate = DateTimeOffset.Now;
                }

                var toDate = fromDate.Value.AddDays(-daysBack);

                var groupPosts = db.Comments.Where(c =>
                    c.GroupId == groupId && c.UpdatedDt <= fromDate && c.UpdatedDt >= toDate
                    && c.ParentCommentId == null) // Only posts no comments
                    .OrderByDescending(x => x.UpdatedDt)
                    .ToList();

               return GetPostDetails(includeAllComments, browserId, groupPosts);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "On GetLatestPostsForGroup: browserId:{browserId} groupId:{groupId} fromDate:{fromDate} daysBack:{daysBack} includeAllComments:{includeAllComments}",
                    browserId, groupId, fromDate, daysBack, includeAllComments);
                Response.StatusCode = 500;
                return null;
            }
        }

        private IList<Post> GetPostDetails(bool includeAllComments, int browserId, List<EF.Comment> groupPosts)
        {
            var result = new List<Post>();

            foreach (var dbPost in groupPosts)
            {
                var dtoPost = new Post();
                dtoPost.GroupId = dbPost.GroupId;
                HandleMsgRead(dbPost, dtoPost, browserId);

                if (includeAllComments)
                {
                    var dbAllPostComments = db.Comments.Where(c =>
                            c.RootCommentId == dbPost.CommentId
                            && c.ParentCommentId != null) // Only comments not the post
                        .ToLookup(x => x.ParentCommentId, x => x);

                    dtoPost.Responses = HandleCommentsRead(dbPost.CommentId, dbAllPostComments, browserId);
                }
                result.Add(dtoPost);
            }
            Response.StatusCode = 200;
            return result;
        }

        private void HandleMsgRead(EF.Comment dbMsg, ApiDtos.Comment dtoMsg, int browserId)
        {
            if (!string.IsNullOrWhiteSpace(dbMsg.LikersJson))
            {
                var likers = JsonSerializer.Deserialize<List<Like>>(dbMsg.LikersJson);
                var dict = new Dictionary<int, int>(likers.Count);
                foreach (Like like in likers)
                {
                    if (dict.TryGetValue(like.LikeType, out int count))
                    {
                        dict[like.LikeType] = ++count;
                    }
                    else
                    {
                        dict[like.LikeType] = 1;
                    }

                    if (like.MemberId == browserId)
                    {
                        dtoMsg.BrowserLikeType = like.LikeType;
                    }
                }
                dtoMsg.LikeType2Count = dict;
            }

            dtoMsg.Id = dbMsg.CommentId;
            dtoMsg.AuthorMemberId = dbMsg.AuthorMemberId;
            dtoMsg.UpdatedDt = dbMsg.UpdatedDt;
            dtoMsg.CreatedDt = dbMsg.CreatedDt;
            dtoMsg.Text = dbMsg.Text;
            dtoMsg.HasMedia = dbMsg.HasMedia;
        }

        private List<ApiDtos.Comment> HandleCommentsRead(long parentId, ILookup<long?, EF.Comment> dbAllPostComments, int browserId)
        {
            var result = new List<ApiDtos.Comment>();
            foreach (var dbComment in dbAllPostComments[parentId].OrderByDescending(x => x.CreatedDt))
            {
                var dtoComment = new ApiDtos.Comment();
                HandleMsgRead(dbComment, dtoComment, browserId);
                dtoComment.Responses = HandleCommentsRead(dbComment.CommentId, dbAllPostComments, browserId);
                result.Add(dtoComment);
            }
            return result;
        }

        [HttpGet("GetEmojiList")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IList<EF6.Emoji> GetEmojiList()
        {
            return db.Emojis.ToList();
        }

        [HttpGet("GetUrlMetadata")]
        public async Task<string> GetUrlMetadata(string url)
        {
            var response = await client.GetAsync("https://api.urlmeta.org/?url=" + url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return null;
        }

        [HttpPost("CreateEmbedded")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task CreateEmbedded(long belongsToPostId, EF.Embedded embedded)
        {
            try
            {
                var apiSession = new ApiSession(this);
                apiSession.Check();
                var post = db.Comments.FirstOrDefault(x => x.CommentId == belongsToPostId);
                if (post == null) throw new Exception("Cannot find post to attach embedded to");
                if (embedded.Url == null)
                {
                    post.EmbeddedUrl = null;
                }
                else
                {
                    post.EmbeddedUrl = JsonSerializer.Serialize(embedded, new JsonSerializerOptions()
                    {
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                    });
                }
                db.SaveChanges();
                Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
            }
        }

        [HttpGet("GetEmbedded")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public ActionResult<string> GetEmbedded(long belongsToPostId)
        {
            try
            {
                //var apiSession = new ApiSession(this);
                //apiSession.Check();
                var post = db.Comments.FirstOrDefault(x => x.CommentId == belongsToPostId);
                return post != null && post.EmbeddedUrl != null ? Ok(JsonSerializer.Serialize(post.EmbeddedUrl)) : NotFound();
                // return JsonSerializer.Serialize(post.EmbeddedUrl) ?? String.Empty;
            }
            catch (Exception ex)
            {
                // Response.StatusCode = 500;
                Console.WriteLine(ex.Message);
                return StatusCode(500);
            }
        }

        [HttpGet("TranslatePost")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<Translation> TranslatePost(long commentId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            var result = await translationService.TranslatePost(commentId);
            Translation translation = new Translation { TranslatedText = result };

            return translation;
        }

        [HttpPost("SetPostHasMedia")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> SetPostHasMedia(long postId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            var post = await db.Comments.FirstOrDefaultAsync(x => x.CommentId == postId);
            if (post != null)
            {
                post.HasMedia = true;
                db.Comments.Update(post);
                await db.SaveChangesAsync();
                return Ok();
            }

            return NotFound();
        }

        [HttpGet("GetPostBlobs")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<List<BlobDto>>> GetPostMedias(int postId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            var post = await db.Comments.FirstOrDefaultAsync(x => x.CommentId == postId);
            if(post is not null)
            {
                if(post.HasMedia != null && post.HasMedia == true)
                {
                    List<BlobDto> blobs = new List<BlobDto>();
                    var container = new BlobContainerClient(_azureConnectionString, _azureContainerName);
                    var medias = container.GetBlobsAsync();
                    
                    await foreach(var blobItem in medias)
                    {
                        if (blobItem.Name.Contains($"{postId}/"))
                        {
                            var uri = container.Uri.AbsoluteUri;
                            var fullUri = uri + "/" + blobItem.Name;
                            var name = blobItem.Name.Replace($"{postId}/", "");

                            blobs.Add(new BlobDto { Name = name, Uri = fullUri, ContentType = blobItem.Properties.ContentType });
                        }
                    }
                    return Ok(blobs);
                }
                else
                {
                    return NotFound($"{postId} does NOT have any media to be viewed");
                }
            }
            return BadRequest("post was not found");
        }
        [HttpPost("DeletePostSingleMediaFile")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeletePostMediaFileByFileName(int postId, string fileName)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            var post = await db.Comments.FirstOrDefaultAsync(x => x.CommentId == postId);
            if (post is not null)
            {
                if (post.HasMedia != null && post.HasMedia == true)
                {
                    var container = new BlobContainerClient(_azureConnectionString, _azureContainerName);
                    await container.DeleteBlobIfExistsAsync($"{postId}/{fileName}");
                    return Ok("file deleted");
                }
                else
                {
                    return NotFound($"{postId} does NOT have any media to be deleted");
                }
            }
            return BadRequest("post was not found");
        }

        [HttpPost("DeletePostAllMediaFiles")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteAllPostMediaFiles(long postId)
        {
            var apiSession = new ApiSession(this);
            apiSession.Check();
            var post = await db.Comments.FirstOrDefaultAsync(x => x.CommentId == postId);
            if (post != null)
            {
                if (post.HasMedia != null && post.HasMedia == true)
                {
                    var container = new BlobContainerClient(_azureConnectionString, _azureContainerName);
                    var medias = container.GetBlobsAsync();

                    await foreach (var blobItem in medias)
                    {
                        if (blobItem.Name.Contains($"{postId}/"))
                        {
                            await container.DeleteBlobIfExistsAsync(blobItem.Name);
                        }
                    }
                    post.HasMedia = false;
                    await db.SaveChangesAsync();
                    return Ok("All files were deleted");
                }
                else
                {
                    return NotFound($"{postId} does NOT have any media to be deleted");
                }
            }
            return BadRequest("post was not found");
        }

    }

    public class BlobDto
    {
        public string Name { get; set; }
        public string Uri { get; set; }
        public string ContentType { get; set; }
    }
}
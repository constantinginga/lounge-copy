using ScSoMe.EF;

namespace ScSoMe.API.Services
{
    public class TrackingMessageService
    {
        private readonly NotificationsService notificationsService;
        public TrackingMessageService()
        {            
            notificationsService = new NotificationsService();
        }

        public async Task<string> SetTracked(long postId, int memberId, bool manual, bool liked, bool commented, bool isPostCreator, bool mentioned, int commentorId, long? commentId = null)
        {
            try
            {
                var db = new ScSoMeContext();
                var now = DateTime.Now;
                var dbMsg = db.TrackedMessages.Where(x => x.PostId == postId && x.MemberId == memberId).FirstOrDefault();
                var dbComment = db.Comments.Where(x => x.AuthorMemberId == memberId).FirstOrDefault();
                if (dbComment != null)
                {
                    isPostCreator = true;
                }
                if (dbMsg == null)
                {
                    var dbTrackedMsg = new EF.TrackedMessage()
                    {
                        MemberId = memberId,
                        PostId = postId,
                        ManualTrack = manual,
                        Liked = liked,
                        Commented = commented,
                        IsPostCreator = isPostCreator,
                        Mentioned = mentioned,
                        InsertedDt = now
                    };
                    await db.TrackedMessages.AddAsync(dbTrackedMsg);
                    await db.SaveChangesAsync();
                    if (commented || mentioned)
                    {
                        if (mentioned)
                        {
                            await notificationsService.CreateNotification(memberId, postId, commented, mentioned, commentorId, commentId);
                        }
                        else
                        {
                            await notificationsService.CreateNotification(memberId, postId, commented, mentioned, 0, commentId);
                        }
                    }

                    return dbTrackedMsg.PostId.ToString() + " is tracked";
                }
                else
                {
                    dbMsg.UpdatedDt = now;

                    if (dbMsg.ManualTrack == false && manual == true)
                        dbMsg.ManualTrack = true;

                    if (dbMsg.Liked == false && liked == true)
                        dbMsg.Liked = true;

                    if (dbMsg.Commented == false && commented == true)
                        dbMsg.Commented = true;

                    if (dbMsg.Mentioned == false && mentioned == true)
                        dbMsg.Mentioned = true;

                    db.TrackedMessages.Update(dbMsg);
                    db.SaveChanges();
                    if (commented || mentioned)
                    {
                        await notificationsService.CreateNotification(memberId, postId, commented, mentioned, commentorId, commentId);
                    }
                    return dbMsg.PostId.ToString() + " is tracked";
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        public async Task<string> RemoveTrackingInfo(long postId, int memberId)
        {
            try
            {
                var db = new ScSoMeContext();
                var dbTrackedMsg = db.TrackedMessages.Where(x => x.PostId == postId && x.MemberId == memberId).FirstOrDefault();
                if (dbTrackedMsg != null)
                    db.TrackedMessages.Remove(dbTrackedMsg);
                await db.SaveChangesAsync();
                return "track removed";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> RemoveMsgTrackInfo(long postId)
        {
            try
            {
                var db = new ScSoMeContext();
                var dbTrackedMsg = db.TrackedMessages.Where(x => x.PostId == postId).ToList();

                if (dbTrackedMsg != null)
                {
                    foreach (var TrackedMsg in dbTrackedMsg)
                    {
                        db.TrackedMessages.Remove(TrackedMsg);
                    }
                }
                await db.SaveChangesAsync();
                return "tracked messages removed";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

    }
}

using ScSoMe.API.Controllers.Members.MembersController;
using ScSoMe.ApiDtos;
using ScSoMe.EF;
using System.Text.Json;

namespace ScSoMe.API.Services
{
    public class NotificationsService
    {
        public const string MentionedYouIn = " mentioned you in ";
        public const string CommentedOnAPostIn = " commented on a post in ";
        public const string CreatedANewPostIn = " created a new post in ";

        static System.Collections.Concurrent.ConcurrentDictionary<int, List<NotificationMessage>> cache_memberId2Notifications = new System.Collections.Concurrent.ConcurrentDictionary<int, List<NotificationMessage>>();

        private readonly PushedNotification _pushedNotification;
        public NotificationsService()
        {
            _pushedNotification = new PushedNotification();
        }
        public async Task CreateNotification(int actorId, long postId, bool comment, bool mention, int commentorId, long? commentId)
        {
            try
            {
                var db = new ScSoMeContext();                
                var post = db.Comments.Where(x => x.CommentId == postId).FirstOrDefault();
                var group = new Group();
                if (post != null)
                    group = db.Groups.Where(x => x.GroupId == post.GroupId).FirstOrDefault();

                string message = "";
                if (post != null && group != null)
                {
                    if (mention && commentorId != 0)
                    {
                        var commentor = MembersController.cache_memberId2info[commentorId];
                        message = commentor.Name + MentionedYouIn + group.Groupname;
                        await AddNotification(postId, message, actorId, true, false, post.GroupId, commentId);
                    }
                    else if (comment && !mention)
                    {
                        var actor = MembersController.cache_memberId2info[actorId];
                        message = actor.Name + CommentedOnAPostIn + group.Groupname;
                        await AddNotification(postId, message, actorId, false, true, post.GroupId, commentId);
                    }
                }
                else
                    message = "someone interacted on post you're following";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task AddNotification(long postId, string message, int actorId, bool isMention, bool isComment, int groupId, long? commentId)
        {
            try
            {
                var db = new ScSoMeContext();
                var subscribers = db.TrackedMessages.Where(x => x.PostId == postId).ToList();
                List<SubscribersJson> subIds = new List<SubscribersJson>();

                foreach (var subscriber in subscribers)
                {
                    if (isMention && subscriber.MemberId == actorId)
                    {
                        subIds.Add(new SubscribersJson { MemberId = subscriber.MemberId, IsRead = false });
                    }
                    else if (isComment && subscriber.MemberId != actorId)
                    {
                        subIds.Add(new SubscribersJson { MemberId = subscriber.MemberId, IsRead = false });
                    }
                    await _pushedNotification.PrepareDeviceNotificaion("Lounge Activity", message, subscriber.MemberId);
                }
                string subscribersJson = JsonSerializer.Serialize(subIds);

                await db.Notifications.AddAsync(new Notification { 
                    NotificationMessage = message, CreatedDate = DateTime.Now, SubscribersJson = subscribersJson, PostId = postId, GroupId = groupId, CommentId = commentId });

                await db.SaveChangesAsync();

                // invalidate the cache after saving to the db
                foreach (var subscriber in subscribers)
                {
                    cache_memberId2Notifications.TryRemove(subscriber.MemberId, out List<NotificationMessage>? value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task AddNotificationForMultipleSubscribersAsync(long postId, string message, IEnumerable<int> subscribers, int groupId, long? commentId)
        {
            List<SubscribersJson> subIds = new List<SubscribersJson>();
            foreach (var subscriber in subscribers)
            {
                subIds.Add(new SubscribersJson { MemberId = subscriber, IsRead = false });
            }
            string subscribersJson = JsonSerializer.Serialize(subIds);

            var db = new ScSoMeContext();

            await db.Notifications.AddAsync(new Notification
            {
                NotificationMessage = message,
                CreatedDate = DateTime.Now,
                SubscribersJson = subscribersJson,
                PostId = postId,
                GroupId = groupId,
                CommentId = commentId,
                EmailedDt = null,
            });

            await db.SaveChangesAsync();

            // invalidate the cache after saving to the db
            foreach (var subscriber in subscribers)
            {
                cache_memberId2Notifications.TryRemove(subscriber, out List<NotificationMessage>? value);
            }
        }


        public List<NotificationMessage> GetNotificationsForMember(int memberId)
        {
            if(cache_memberId2Notifications.TryGetValue(memberId, out List<NotificationMessage>? result)) 
            {
                return result;
            }

            List<SubscribersJson>? subscribers = new List<SubscribersJson>();
            List<NotificationMessage> messages = new List<NotificationMessage>();

            try
            {
                var db = new ScSoMeContext();
                List<Notification>? notifications = db.Notifications.OrderByDescending(x => x.CreatedDate).ToList();
                
                foreach (var record in notifications)
                {
                    if (!string.IsNullOrWhiteSpace(record.SubscribersJson))
                    {
                        subscribers = JsonSerializer.Deserialize<List<SubscribersJson>>(record.SubscribersJson);

                        if (subscribers != null)
                        {
                            foreach (var subscriber in subscribers)
                            {
                                if (subscriber.MemberId == memberId && !string.IsNullOrEmpty(record.NotificationMessage))
                                {                                    
                                    messages.Add(new NotificationMessage { 
                                        NotificationId = record.NotificationId, 
                                        Message = record.NotificationMessage.ToString(), 
                                        PostId = record.PostId.Value, 
                                        IsRead = subscriber.IsRead,
                                        GroupId = record.GroupId,
                                        CommentId = record.CommentId,
                                        CreatedDate = record.CreatedDate
                                    });
                                    break;
                                }
                            }
                        }
                    }
                }

                cache_memberId2Notifications.TryAdd(memberId, messages);
                return messages;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                messages.Add(new NotificationMessage { Message = ex.Message });
                return messages;
            }
        }

        public async Task<bool> RemoveSubscriberNotification(int notificationId, int subscriberId)
        {
            try
            {
                var db = new ScSoMeContext();
                var notification = db.Notifications.Where(x => x.NotificationId == notificationId).FirstOrDefault();
                List<SubscribersJson>? newSubscribersList = new List<SubscribersJson>();
                if (notification != null)
                {
                    string newSubscribersString = "";

                    if (!string.IsNullOrWhiteSpace(notification.SubscribersJson))
                    {
                        var subsList = JsonSerializer.Deserialize<List<SubscribersJson>>(notification.SubscribersJson);
                        if (subsList != null)
                        {
                            foreach (var subscriber in subsList)
                            {
                                if (subscriber.MemberId != subscriberId)
                                {
                                    newSubscribersList.Add(subscriber);
                                }
                            }

                            newSubscribersString = JsonSerializer.Serialize(newSubscribersList);
                            if (string.IsNullOrWhiteSpace(newSubscribersString) || newSubscribersString.Equals("[]"))
                            {
                                db.Remove(notification);
                                await db.SaveChangesAsync();
                            }
                            else
                            {
                                notification.SubscribersJson = newSubscribersString;

                                db.Update(notification);
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                }

                // invalidate the cache after saving to the db
                cache_memberId2Notifications.TryRemove(subscriberId, out List<NotificationMessage>? cachedNotifications);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public async Task RemoveNotifications(long postId)
        {
            var db = new ScSoMeContext();
            var notifications = db.Notifications.Where(x => x.PostId == postId).ToList();
            if (notifications != null)
            {
                foreach (var notification in notifications)
                {
                    db.Notifications.Remove(notification);

                    await db.SaveChangesAsync();

                    var subsList = JsonSerializer.Deserialize<List<SubscribersJson>>(notification.SubscribersJson);
                    if (subsList != null)
                    {
                        foreach (var subscriber in subsList)
                        {
                            // invalidate the cache after saving to the db
                            cache_memberId2Notifications.TryRemove(subscriber.MemberId, out List<NotificationMessage>? cachedNotifications);
                        }
                    }
                }
            }
        }


        public async Task SetNewPostNotificationAsync(int groupId, long postId, int authorId)
        {
            var db = new ScSoMeContext();
            //var groupSubscribers = db.GroupsReads.Where(x => x.GroupId == groupId && x.NotifyOnNew && x.MemberId != authorId);
            var groupSubscribers = db.GroupsReads.Where(x => x.GroupId == groupId && x.MemberId != authorId);
            if (!groupSubscribers.Any())
            {
                return;
                
            }
            
            var groupTask = db.Groups.FindAsync(groupId);
            var memberName = MembersController.cache_memberId2info[authorId].Name;
            await groupTask;
            foreach (var subscriber in groupSubscribers)
            {
                await _pushedNotification.PrepareDeviceNotificaion("Lounge Activity", memberName + CreatedANewPostIn + groupTask.Result.Groupname, subscriber.MemberId);
            }
            await AddNotificationForMultipleSubscribersAsync(postId,
                memberName + CreatedANewPostIn + groupTask.Result.Groupname, groupSubscribers.Select(x => x.MemberId),
                groupId, null);
        }
        
        public async Task ReadNotification(int notificationId, int memberId)
        {            
            var db = new ScSoMeContext();

            var notification = db.Notifications.Where(x => x.NotificationId == notificationId).FirstOrDefault();
            if (notification != null && !string.IsNullOrWhiteSpace(notification.SubscribersJson))
            {
                string newSubscribersString = "";

                var subsList = JsonSerializer.Deserialize<List<SubscribersJson>>(notification.SubscribersJson);
                if (subsList != null)
                {
                    foreach (var subscriber in subsList)
                    {
                        if (subscriber.MemberId == memberId && !subscriber.IsRead)
                        {
                            subscriber.IsRead = true;
                            break;
                        }
                    }
                    newSubscribersString = JsonSerializer.Serialize(subsList);

                    notification.SubscribersJson = newSubscribersString;

                    db.Update(notification);
                    await db.SaveChangesAsync();

                    // update the cache after saving to the db
                    cache_memberId2Notifications.TryGetValue(memberId, out List<NotificationMessage>? cachedNotifications);
                    if (cachedNotifications != null)
                    {
                        var cachedNotification = cachedNotifications.SingleOrDefault(x => x.NotificationId == notificationId);
                        if (cachedNotification != null)
                            cachedNotification.IsRead = true;
                    }
                }
            }
        }

        public async Task ReadAllNotifications(int memberId, List<NotificationMessage> msgs)
        {
            try
            {
                var db = new ScSoMeContext();
                foreach (var notification in msgs)
                {
                    var dbNotification = db.Notifications.Where(x => x.NotificationId == notification.NotificationId).FirstOrDefault();
                    if (dbNotification != null && !string.IsNullOrWhiteSpace(dbNotification.SubscribersJson))
                    {
                        string newSubscribersString = "";

                        var subsList = JsonSerializer.Deserialize<List<SubscribersJson>>(dbNotification.SubscribersJson);
                        if (subsList != null)
                        {
                            foreach (var subscriber in subsList)
                            {
                                if (subscriber.MemberId == memberId && !subscriber.IsRead)
                                {
                                    subscriber.IsRead = true;
                                }
                            }
                            newSubscribersString = JsonSerializer.Serialize(subsList);

                            dbNotification.SubscribersJson = newSubscribersString;

                            db.Update(dbNotification);
                        }
                    }
                }

                await db.SaveChangesAsync();

                // invalidate the cache after saving to the db
                cache_memberId2Notifications.TryRemove(memberId, out List<NotificationMessage>? cachedNotifications);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        
    }
}

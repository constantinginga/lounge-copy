using ScSoMe.API.Controllers.Members.MembersController;
using ScSoMe.ApiDtos;
using System.Text;

namespace ScSoMe.API.Services
{
    public class EmailNotificationsService
    {
        private static PeriodicTimer periodicTimer;

        internal static async Task CheckQueueAndSendAsync()
        {
            if (!MembersController.cache_memberId2info.Any())
            {
                MembersController.UpdateCache();
            }

            var db = new EF.ScSoMeContext();

            var now = DateTime.Now;
            var daysAgo = now.AddDays(-1);

            var queuedNotifications = db.Notifications.Where(x => x.CreatedDate >= daysAgo && x.EmailedDt == null);
            //if(queuedNotifications.Any()) Console.WriteLine("queuedNotifications " + queuedNotifications.Count());

            var mailMembers = new Dictionary<int, List<EF.Notification>>();

            var emailSubscriptions = new Dictionary<int, EmailSubscriptionService.EmailSubscriptions>();
            var emailSubscriptionService = new EmailSubscriptionService(/*if this service caches then skip the emailSubscriptions dict.*/);

            foreach (var notification in queuedNotifications)
            {
                if (string.IsNullOrWhiteSpace(notification.SubscribersJson)) continue;

                var subscribers = System.Text.Json.JsonSerializer.Deserialize<List<SubscribersJson>>(notification.SubscribersJson);
                foreach (var subscriber in subscribers)
                {
                    if (notification.NotificationMessage == null) continue;

                    if (!subscriber.IsRead)
                    {
                        mailMembers.TryGetValue(subscriber.MemberId, out List<EF.Notification> list);
                        if (list == null)
                        {
                            list = new List<EF.Notification>();
                            mailMembers[subscriber.MemberId] = list;
                        }
                                                                       
                        if (!emailSubscriptions.TryGetValue(subscriber.MemberId, out EmailSubscriptionService.EmailSubscriptions subscription))
                        {
                            subscription = await emailSubscriptionService.GetSubscriptions(subscriber.MemberId);
                            emailSubscriptions[subscriber.MemberId] = subscription;
                        }
                        var isNewPost = notification.NotificationMessage.Contains(NotificationsService.CreatedANewPostIn);
                        if (isNewPost && subscription.NewPosts)
                            list.Add(notification);
                        else
                        {
                            var isComment = notification.NotificationMessage.Contains(NotificationsService.CommentedOnAPostIn);
                            if (isComment && subscription.Comments)
                                list.Add(notification);
                            else
                            {
                                var isMention = notification.NotificationMessage.Contains(NotificationsService.MentionedYouIn);
                                if (isMention && subscription.Mentions)
                                    list.Add(notification);
                            }
                        }
                    }
                }
            }

            foreach (var mailMember in mailMembers)
            {
                StringBuilder sb = new StringBuilder();
                var member = MembersController.cache_memberId2info[mailMember.Key];
                
                if (!member.IsApproved) continue; // skip un-approved members

                var notificationsGroupedByPostId = mailMember.Value.Where(x => x.PostId.HasValue)
                    .GroupBy(x => x.PostId.Value);

                if (!notificationsGroupedByPostId.Any()) continue; // skip member when there are no notifications to email

                foreach (var notifications in notificationsGroupedByPostId)
                {
                    var postId = notifications.Key;
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

                    var orderedNotifications = notifications.OrderBy(x => x.CreatedDate);
                    var firstNotification = orderedNotifications.First();
                    if (!firstNotification.CommentId.HasValue) // is post - not a comment
                    {
                        sb.AppendLine("<ul><li><i>");
                        sb.AppendLine(firstNotification.CreatedDate.Value.ToString("yyyy-MM-dd HH:mm"));
                        sb.AppendLine(firstNotification.NotificationMessage);
                        sb.AppendLine("</i></li></ul><div>");
                        var msg = postMessage.Text;//.TakeWhile(x => x != '<');
                        var maxLength = Math.Min(msg.Count(), 350);
                        sb.AppendLine(postMessage.Text.Substring(0, maxLength));
                        if (maxLength == 350) sb.Append("...");
                        sb.AppendLine("</div>");
                        orderedNotifications = orderedNotifications.Skip(1).OrderBy(x => x.CreatedDate);
                    }
                    if (orderedNotifications.Any())
                    {
                        sb.AppendLine("<ul>");
                        foreach (var notification in notifications.OrderBy(x => x.CreatedDate))
                        {
                            if (!notification.CommentId.HasValue) continue; // a comment without CommentId
                            sb.Append("<li>");
                            sb.AppendLine("<i>");
                            sb.AppendLine(notification.CreatedDate.Value.ToString("yyyy-MM-dd HH:mm"));
                            sb.AppendLine(notification.NotificationMessage);
                            sb.AppendLine("</i>");
                            sb.AppendLine("<br/><div>");
                            var commentMessage = db.Comments.Single(x => x.CommentId == notification.CommentId);
                            var msg = commentMessage.Text;//.TakeWhile(x => x != '<');
                            var maxLength = Math.Min(msg.Count(), 350);
                            sb.AppendLine(commentMessage.Text.Substring(0, maxLength));
                            if (maxLength == 350) sb.Append("...");
                            sb.Append("</div></li>");
                        }
                        sb.AppendLine("</ul>");
                    }
                    sb.AppendLine("<br/>");
                    sb.AppendLine("<br/>");
                }

                sb.AppendLine("<a href=\"https://www.startupcentral.dk/Lounge/emailsubscriptions\"> Update your email preferences</a>");

                //await EmailService.SendMailAsync("costelgn@gmail.com", 
                //    //member.Login,
                //    null, "New Lounge | Activity | " + now.ToString("yyyy-MM-dd HH:mm"), sb.ToString(), null);
                
                if (!isProd)
                    File.WriteAllText(now.ToString("O").Replace(":", "") + "-" + member.Login + ".html", sb.ToString());
            }

            foreach (var notification in queuedNotifications)
            {
                notification.EmailedDt = now;
            }
            //db.Notifications.UpdateRange(queuedNotifications);
            //await db.SaveChangesAsync();
        }

        static DateTime nextDateTime = DateTime.Now;
        static bool isProd = Environment.MachineName.Equals("startupVM");

        internal static async Task StartPeriodicTimerAsync()
        {
            periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(7));
            _ = Task.Factory.StartNew(async () =>
            {
                if (isProd)
                {
                    var maxEmailed = new ScSoMe.EF.ScSoMeContext().Notifications.Max(x => x.EmailedDt);
                    if (maxEmailed.HasValue)
                    {
                        nextDateTime = maxEmailed.Value.AddMinutes(60 - maxEmailed.Value.Minute);
                    }
                    else
                    {
                        nextDateTime = DateTime.Now.AddMinutes(60 - DateTime.Now.Minute);
                    }
                }

                //Console.WriteLine("nextDateTime : " + nextDateTime);

                while (await periodicTimer.WaitForNextTickAsync())
                {                    
                    var now = DateTime.Now;
                    //Console.WriteLine(now + " checking");
                    if (now > nextDateTime)
                    {
                        try
                        {
                            await CheckQueueAndSendAsync();
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine(err);
                        }

                        nextDateTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
                        nextDateTime = nextDateTime.AddHours(1);
                    }
                }
            });
        }
    }
}

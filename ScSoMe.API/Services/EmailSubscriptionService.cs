namespace ScSoMe.API.Services
{
    public class EmailSubscriptionService
    {
        /// <summary>
        /// True means that the member want the emails
        /// </summary>
        public class EmailSubscriptions
        {
            public bool NewPosts { get; set; }
            public bool Comments { get; set; }
            public bool Mentions { get; set; }
        }


        public async Task SetSubscriptions(int memberId, EmailSubscriptions subscription)
        {
            var db = new ScSoMe.EF.ScSoMeContext();
            var existingRow = await db.MemberEmailSubscriptions.FindAsync(memberId);
            if (existingRow == null)
            {
                await db.MemberEmailSubscriptions.AddAsync(new EF.MemberEmailSubscription()
                {
                    MemberId = memberId,
                    NewPosts = subscription.NewPosts,
                    Comments = subscription.Comments,
                    Mentions = subscription.Mentions
                });
            }
            else
            {
                existingRow.NewPosts = subscription.NewPosts;
                existingRow.Comments = subscription.Comments;
                existingRow.Mentions = subscription.Mentions;
            }
            await db.SaveChangesAsync();
        }

        public async Task<EmailSubscriptions> GetSubscriptions(int memberId)
        {
            var db = new ScSoMe.EF.ScSoMeContext();
            var existingRow = await db.MemberEmailSubscriptions.FindAsync(memberId);
            if (existingRow == null)
            {
                return new EmailSubscriptions() { NewPosts = true, Comments = true, Mentions = true };
            }
            else
            {
                return new EmailSubscriptions() { NewPosts = existingRow.NewPosts, Comments = existingRow.Comments, Mentions = existingRow.Mentions };
            }
        }
    }
}

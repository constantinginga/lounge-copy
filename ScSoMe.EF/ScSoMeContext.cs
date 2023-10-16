using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ScSoMe.EF
{
    public partial class ScSoMeContext : DbContext
    {
        public ScSoMeContext()
        {
        }

        public ScSoMeContext(DbContextOptions<ScSoMeContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ActiveMember> ActiveMembers { get; set; } = null!;
        public virtual DbSet<ActivitySection> ActivitySections { get; set; } = null!;
        public virtual DbSet<BlockedMember> BlockedMembers { get; set; } = null!;
        public virtual DbSet<Chat> Chats { get; set; } = null!;
        public virtual DbSet<Comment> Comments { get; set; } = null!;
        public virtual DbSet<ContactsSection> ContactsSections { get; set; } = null!;
        public virtual DbSet<CountryCode> CountryCodes { get; set; } = null!;
        public virtual DbSet<DescriptionSection> DescriptionSections { get; set; } = null!;
        public virtual DbSet<Emoji> Emojis { get; set; } = null!;
        public virtual DbSet<ExternalLink> ExternalLinks { get; set; } = null!;
        public virtual DbSet<ExternalLinksSection> ExternalLinksSections { get; set; } = null!;
        public virtual DbSet<Group> Groups { get; set; } = null!;
        public virtual DbSet<GroupsRead> GroupsReads { get; set; } = null!;
        public virtual DbSet<Member> Members { get; set; } = null!;
        public virtual DbSet<MemberDeviceToken> MemberDeviceTokens { get; set; } = null!;
        public virtual DbSet<MemberEmailSubscription> MemberEmailSubscriptions { get; set; } = null!;
        public virtual DbSet<MemberNetPromotorScore> MemberNetPromotorScores { get; set; } = null!;
        public virtual DbSet<MemberToken> MemberTokens { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<Participant> Participants { get; set; } = null!;
        public virtual DbSet<PostFirstRead> PostFirstReads { get; set; } = null!;
        public virtual DbSet<ServicesSection> ServicesSections { get; set; } = null!;
        public virtual DbSet<TrackedMessage> TrackedMessages { get; set; } = null!;
        public virtual DbSet<Translation> Translations { get; set; } = null!;
        public virtual DbSet<WorkExperience> WorkExperiences { get; set; } = null!;
        public virtual DbSet<WorkExperienceSection> WorkExperienceSections { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=localhost;Database=ScSoMe;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ActiveMember>(entity =>
            {
                entity.HasKey(e => new { e.MemberId, e.LoginDate })
                    .HasName("PK__ActiveMe__5DE43ACF109BA182");

                entity.Property(e => e.LoginDate).HasPrecision(6);
            });

            modelBuilder.Entity<ActivitySection>(entity =>
            {
                entity.HasKey(e => e.MemberId)
                    .HasName("PK__Activity__B29B85340111587B");

                entity.ToTable("ActivitySection");

                entity.Property(e => e.MemberId)
                    .ValueGeneratedNever()
                    .HasColumnName("member_id");

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content");

                entity.Property(e => e.PrivacySetting).HasColumnName("privacy_setting");

                entity.HasOne(d => d.Member)
                    .WithOne(p => p.ActivitySection)
                    .HasForeignKey<ActivitySection>(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ActivityS__membe__09746778");
            });

            modelBuilder.Entity<BlockedMember>(entity =>
            {
                entity.HasKey(e => new { e.MemberId, e.BlockedMemberId })
                    .HasName("PK__BlockedM__3845C6010FB9DA39");
            });

            modelBuilder.Entity<Chat>(entity =>
            {
                entity.Property(e => e.ChatId).HasColumnName("chat_id");

                entity.Property(e => e.Chatgroupname).HasColumnName("chatgroupname");

                entity.Property(e => e.CreatedDt)
                    .HasPrecision(6)
                    .HasColumnName("created_dt");

                entity.Property(e => e.Displayname).HasColumnName("displayname");

                entity.Property(e => e.Newdisplayname).HasColumnName("newdisplayname");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.CommentId)
                    .IsClustered(false);

                entity.HasIndex(e => e.RootCommentId, "IX_ClusteredIndex-RootComment")
                    .IsClustered();

                entity.HasIndex(e => new { e.GroupId, e.ParentCommentId }, "IX_Group_Updated")
                    .HasFillFactor(70);

                entity.Property(e => e.CommentId)
                    .ValueGeneratedNever()
                    .HasColumnName("comment_id");

                entity.Property(e => e.AuthorMemberId).HasColumnName("author_member_id");

                entity.Property(e => e.CreatedDt).HasColumnName("created_dt");

                entity.Property(e => e.EmbeddedUrl)
                    .HasColumnName("embedded_url")
                    .IsSparse();

                entity.Property(e => e.GroupId).HasColumnName("group_id");

                entity.Property(e => e.HasMedia).HasColumnName("has_media");

                entity.Property(e => e.LikersJson)
                    .IsUnicode(false)
                    .HasColumnName("likers_json");

                entity.Property(e => e.ParentCommentId).HasColumnName("parent_comment_id");

                entity.Property(e => e.RootCommentId)
                    .HasColumnName("root_comment_id")
                    .HasComment("Null for the post - and post id for every comment");

                entity.Property(e => e.Text).HasColumnName("text");

                entity.Property(e => e.UpdatedDt).HasColumnName("updated_dt");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comments_Groups");
            });

            modelBuilder.Entity<ContactsSection>(entity =>
            {
                entity.HasKey(e => e.MemberId)
                    .HasName("PK__Contacts__B29B8534BAC58125");

                entity.ToTable("ContactsSection");

                entity.Property(e => e.MemberId)
                    .ValueGeneratedNever()
                    .HasColumnName("member_id");

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content");

                entity.Property(e => e.PrivacySetting).HasColumnName("privacy_setting");

                entity.HasOne(d => d.Member)
                    .WithOne(p => p.ContactsSection)
                    .HasForeignKey<ContactsSection>(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ContactsS__membe__7E02B4CC");
            });

            modelBuilder.Entity<CountryCode>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DialCode).HasColumnName("dial_code");

                entity.Property(e => e.Flag).HasColumnName("flag");

                entity.Property(e => e.Name).HasColumnName("name");

                entity.Property(e => e.ShortName).HasColumnName("short_name");
            });

            modelBuilder.Entity<DescriptionSection>(entity =>
            {
                entity.HasKey(e => e.MemberId)
                    .HasName("PK__Descript__B29B85345A4006B9");

                entity.ToTable("DescriptionSection");

                entity.Property(e => e.MemberId)
                    .ValueGeneratedNever()
                    .HasColumnName("member_id");

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content");

                entity.Property(e => e.PrivacySetting).HasColumnName("privacy_setting");

                entity.HasOne(d => d.Member)
                    .WithOne(p => p.DescriptionSection)
                    .HasForeignKey<DescriptionSection>(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Descripti__membe__756D6ECB");
            });

            modelBuilder.Entity<Emoji>(entity =>
            {
                entity.Property(e => e.EmojiId).HasColumnName("emoji_id");

                entity.Property(e => e.Category).HasColumnName("category");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.EmojiIcon).HasColumnName("emoji_icon");
            });

            modelBuilder.Entity<ExternalLink>(entity =>
            {
                entity.Property(e => e.ExternalLinkId)
                    .ValueGeneratedNever()
                    .HasColumnName("externalLinkId");

                entity.Property(e => e.MemberId).HasColumnName("member_id");

                entity.Property(e => e.Title)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("title");

                entity.Property(e => e.Url)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("url");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.ExternalLinks)
                    .HasForeignKey(d => d.MemberId)
                    .HasConstraintName("FK__ExternalL__membe__7B264821");
            });

            modelBuilder.Entity<ExternalLinksSection>(entity =>
            {
                entity.HasKey(e => e.MemberId)
                    .HasName("PK__External__B29B85342B3F1040");

                entity.ToTable("ExternalLinksSection");

                entity.Property(e => e.MemberId)
                    .ValueGeneratedNever()
                    .HasColumnName("member_id");

                entity.Property(e => e.PrivacySetting).HasColumnName("privacy_setting");

                entity.HasOne(d => d.Member)
                    .WithOne(p => p.ExternalLinksSection)
                    .HasForeignKey<ExternalLinksSection>(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ExternalL__membe__7849DB76");
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.Property(e => e.GroupId)
                    .ValueGeneratedNever()
                    .HasColumnName("group_id");

                entity.Property(e => e.CreatedDt).HasColumnName("created_dt");

                entity.Property(e => e.Groupname)
                    .IsUnicode(false)
                    .HasColumnName("groupname");

                entity.Property(e => e.UpdatedDt).HasColumnName("updated_dt");

                entity.Property(e => e.Url)
                    .IsUnicode(false)
                    .HasColumnName("url");
            });

            modelBuilder.Entity<GroupsRead>(entity =>
            {
                entity.HasKey(e => new { e.MemberId, e.GroupId });

                entity.ToTable("GroupsRead");

                entity.Property(e => e.MemberId).HasColumnName("member_id");

                entity.Property(e => e.GroupId).HasColumnName("group_id");

                entity.Property(e => e.LastReadDt)
                    .HasPrecision(6)
                    .HasColumnName("last_read_dt");

                entity.Property(e => e.NotifyOnNew).HasColumnName("notify_on_new");
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.Property(e => e.MemberId)
                    .ValueGeneratedNever()
                    .HasColumnName("member_id");

                entity.Property(e => e.CreatedDt).HasColumnName("created_dt");

                entity.Property(e => e.Email)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.Json)
                    .IsUnicode(false)
                    .HasColumnName("json");

                entity.Property(e => e.Login)
                    .IsUnicode(false)
                    .HasColumnName("login");

                entity.Property(e => e.Name)
                    .IsUnicode(false)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.UpdatedDt).HasColumnName("updated_dt");

                entity.Property(e => e.Url)
                    .IsUnicode(false)
                    .HasColumnName("url");
            });

            modelBuilder.Entity<MemberDeviceToken>(entity =>
            {
                entity.HasKey(e => new { e.MemberId, e.DeviceToken })
                    .HasName("PK__MemberDe__756ECDD46148543B");

                entity.Property(e => e.DeviceToken)
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MemberEmailSubscription>(entity =>
            {
                entity.HasKey(e => e.MemberId);

                entity.Property(e => e.MemberId)
                    .ValueGeneratedNever()
                    .HasColumnName("member_id");

                entity.Property(e => e.Comments).HasColumnName("comments");

                entity.Property(e => e.Mentions).HasColumnName("mentions");

                entity.Property(e => e.NewPosts).HasColumnName("new_posts");
            });

            modelBuilder.Entity<MemberNetPromotorScore>(entity =>
            {
                entity.HasKey(e => new { e.MemberId, e.ReportDate })
                    .HasName("PK__MemberNe__84D67336BE5B088C");

                entity.ToTable("MemberNetPromotorScore");

                entity.Property(e => e.ReportDate).HasPrecision(6);

                entity.Property(e => e.Nps).HasColumnName("nps");

                entity.Property(e => e.Sugestion)
                    .HasColumnName("sugestion")
                    .IsSparse();
            });

            modelBuilder.Entity<MemberToken>(entity =>
            {
                entity.HasKey(e => e.DeviceId);

                entity.HasIndex(e => e.Token, "IX_MemberTokens_Token")
                    .IsUnique();

                entity.Property(e => e.DeviceId)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("DeviceID");

                entity.Property(e => e.CreatedDt).HasColumnType("smalldatetime");

                entity.Property(e => e.MemberId).HasColumnName("MemberID");

                entity.Property(e => e.Token)
                    .HasMaxLength(850)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedDt).HasColumnType("smalldatetime");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.Property(e => e.MessageId).HasColumnName("message_id");

                entity.Property(e => e.ChatId).HasColumnName("chat_id");

                entity.Property(e => e.CreatedDt)
                    .HasPrecision(6)
                    .HasColumnName("created_dt");

                entity.Property(e => e.IsRead)
                    .HasMaxLength(6)
                    .HasColumnName("isRead");

                entity.Property(e => e.MediaUrl).HasColumnName("media_url");

                entity.Property(e => e.SenderMemberId).HasColumnName("sender_member_id");

                entity.Property(e => e.SenderName)
                    .HasMaxLength(255)
                    .HasColumnName("sender_name");

                entity.Property(e => e.Text).HasColumnName("text");

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.ChatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Messages_Chat");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.EmailedDt)
                    .HasPrecision(6)
                    .HasColumnName("emailed_dt");

                entity.Property(e => e.NotificationMessage).IsUnicode(false);

                entity.Property(e => e.SubscribersJson)
                    .IsUnicode(false)
                    .HasColumnName("Subscribers_json");
            });

            modelBuilder.Entity<Participant>(entity =>
            {
                entity.HasIndex(e => new { e.ChatId, e.MemberId }, "UQ__Particip__A62DB345D32C878C")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ChatId).HasColumnName("chat_id");

                entity.Property(e => e.MemberId).HasColumnName("member_id");

                entity.HasOne(d => d.Chat)
                    .WithMany(p => p.Participants)
                    .HasForeignKey(d => d.ChatId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Participants_Chats");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.Participants)
                    .HasForeignKey(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Participants_Members");
            });

            modelBuilder.Entity<PostFirstRead>(entity =>
            {
                entity.HasKey(e => new { e.RootCommentId, e.MemberId });

                entity.ToTable("PostFirstRead");

                entity.Property(e => e.RootCommentId).HasColumnName("root_comment_id");

                entity.Property(e => e.MemberId).HasColumnName("member_id");

                entity.Property(e => e.FirstDt)
                    .HasPrecision(6)
                    .HasColumnName("first_dt");
            });

            modelBuilder.Entity<ServicesSection>(entity =>
            {
                entity.HasKey(e => e.MemberId)
                    .HasName("PK__Services__B29B85345EACE655");

                entity.ToTable("ServicesSection");

                entity.Property(e => e.MemberId)
                    .ValueGeneratedNever()
                    .HasColumnName("member_id");

                entity.Property(e => e.Content)
                    .HasColumnType("text")
                    .HasColumnName("content");

                entity.Property(e => e.PrivacySetting).HasColumnName("privacy_setting");

                entity.HasOne(d => d.Member)
                    .WithOne(p => p.ServicesSection)
                    .HasForeignKey<ServicesSection>(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ServicesS__membe__00DF2177");
            });

            modelBuilder.Entity<TrackedMessage>(entity =>
            {
                entity.HasKey(e => new { e.MemberId, e.PostId })
                    .HasName("PK__TrackedM__96516D19C39E0536");

                entity.Property(e => e.InsertedDt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedDt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Translation>(entity =>
            {
                entity.HasKey(e => new { e.CommentId, e.LanguageCode })
                    .HasName("PK__Translat__8B0C1769ED4E153D");

                entity.Property(e => e.LanguageCode)
                    .HasMaxLength(2)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<WorkExperience>(entity =>
            {
                entity.ToTable("WorkExperience");

                entity.Property(e => e.WorkExperienceId)
                    .ValueGeneratedNever()
                    .HasColumnName("workExperienceId");

                entity.Property(e => e.CompanyName)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("companyName");

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .HasColumnName("endDate");

                entity.Property(e => e.MemberId).HasColumnName("member_id");

                entity.Property(e => e.Position)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("position");

                entity.Property(e => e.PositionDescription)
                    .HasColumnType("text")
                    .HasColumnName("positionDescription");

                entity.Property(e => e.StartDate)
                    .HasColumnType("date")
                    .HasColumnName("startDate");

                entity.HasOne(d => d.Member)
                    .WithMany(p => p.WorkExperiences)
                    .HasForeignKey(d => d.MemberId)
                    .HasConstraintName("FK__WorkExper__membe__0697FACD");
            });

            modelBuilder.Entity<WorkExperienceSection>(entity =>
            {
                entity.HasKey(e => e.MemberId)
                    .HasName("PK__WorkExpe__B29B85348D9F0D78");

                entity.ToTable("WorkExperienceSection");

                entity.Property(e => e.MemberId)
                    .ValueGeneratedNever()
                    .HasColumnName("member_id");

                entity.Property(e => e.PrivacySetting).HasColumnName("privacy_setting");

                entity.HasOne(d => d.Member)
                    .WithOne(p => p.WorkExperienceSection)
                    .HasForeignKey<WorkExperienceSection>(d => d.MemberId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__WorkExper__membe__03BB8E22");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

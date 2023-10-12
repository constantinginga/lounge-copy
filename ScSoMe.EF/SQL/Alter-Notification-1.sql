
ALTER TABLE [dbo].[Notification]
    ADD [emailed_dt] DATETIME2 (6) NULL,
        [GroupId]    INT           NULL,
        [CommentId]  BIGINT        NULL;


GO

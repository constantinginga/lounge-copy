USE [scSoMe]
GO

/****** Object:  Table [dbo].[MemberTokens]    Script Date: 29-04-2022 11:53:22 ******/
/*
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MemberTokens]') AND type in (N'U'))
DROP TABLE [dbo].[MemberTokens]
GO
*/

/****** Object:  Table [dbo].[MemberTokens]    Script Date: 29-04-2022 11:53:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MemberTokens](
	[Token] [varchar](850) NOT NULL,
	[MemberID] [int] NOT NULL,
	[DeviceID] [varchar](50) NOT NULL,
	[CreatedDt] [smalldatetime] NOT NULL,
	[UpdatedDt] [smalldatetime] NULL,
 CONSTRAINT [PK_MemberTokens] PRIMARY KEY CLUSTERED 
(
	[DeviceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


/****** Object:  Index [IX_MemberTokens_Token]    Script Date: 29-04-2022 11:53:54 ******/
/*
DROP INDEX [IX_MemberTokens_Token] ON [dbo].[MemberTokens]
GO
*/

/****** Object:  Index [IX_MemberTokens_Token]    Script Date: 29-04-2022 11:53:55 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_MemberTokens_Token] ON [dbo].[MemberTokens]
(
	[Token] ASC
)
INCLUDE([MemberID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

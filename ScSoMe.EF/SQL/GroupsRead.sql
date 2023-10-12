USE [scSoMe]
GO

/* ***** Object:  Table [dbo].[GroupsRead]    Script Date: 02-05-2022 18:43:15 ****** /
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GroupsRead]') AND type in (N'U'))
DROP TABLE [dbo].[GroupsRead]
GO
*/

/****** Object:  Table [dbo].[GroupsRead]    Script Date: 02-05-2022 19:07:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[GroupsRead](
	[member_id] [int] NOT NULL,
	[group_id] [int] NOT NULL,
	[last_read_dt] [datetime2](6) NOT NULL,
	[notify_on_new] [bit] NOT NULL,
 CONSTRAINT [PK_GroupsRead] PRIMARY KEY CLUSTERED 
(
	[member_id] ASC,
	[group_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO






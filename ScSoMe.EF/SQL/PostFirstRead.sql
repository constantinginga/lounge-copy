USE [scSoMe]
GO

/****** Object:  Table [dbo].[PostFirstRead]    Script Date: 23-06-2022 11:43:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PostFirstRead](
	[root_comment_id] [bigint] NOT NULL,
	[member_id] [int] NOT NULL,
	[first_dt] [datetime2](6) NOT NULL,
 CONSTRAINT [PK_PostFirstRead] PRIMARY KEY CLUSTERED 
(
	[root_comment_id] ASC,
	[member_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO



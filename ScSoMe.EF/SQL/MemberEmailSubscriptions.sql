USE [scSoMe]
GO

/****** Object:  Table [dbo].[MemberEmailSubscriptions]    Script Date: 16-06-2022 10:13:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MemberEmailSubscriptions](
	[member_id] [int] NOT NULL,
	[new_posts] [bit] NOT NULL,
	[comments] [bit] NOT NULL,
	[mentions] [bit] NOT NULL,
 CONSTRAINT [PK_MemberEmailSubscriptions] PRIMARY KEY CLUSTERED 
(
	[member_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO



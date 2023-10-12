USE [scSoMe]
GO

/****** Object:  Table [dbo].[MemberNetPromotorScore]    Script Date: 02-08-2022 14:04:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MemberNetPromotorScore](
	[MemberId] [int] NOT NULL,
	[ReportDate] [datetime2](6) NOT NULL,
	[nps] [tinyint] NOT NULL,
	[sugestion] [nvarchar](max) SPARSE  NULL,
PRIMARY KEY CLUSTERED 
(
	[MemberId] ASC,
	[ReportDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO



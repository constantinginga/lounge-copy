SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


Create TABLE BlockedMembers
(
MemberId INT NOT NULL,
BlockedMemberId INT NOT NULL,

--Composite primary key

PRIMARY KEY (MemberId, BlockedMemberId)
);
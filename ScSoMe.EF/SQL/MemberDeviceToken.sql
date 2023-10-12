
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create TABLE MemberDeviceTokens
(
MemberId INT NOT NULL,
DeviceToken VARCHAR(500) NOT NULL,
LoggedOut BIT,
--Composite primary key

PRIMARY KEY (MemberId, DeviceToken)
);
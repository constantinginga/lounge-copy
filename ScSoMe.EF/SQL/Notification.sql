CREATE TABLE [Notification] (
    NotificationId INT NOT NULL IDENTITY(1,1),
    NotificationMessage VARCHAR(MAX),
    CreatedDate DATETIME,
    Subscribers_json VARCHAR(MAX),
	PostId BIGINT,
    PRIMARY KEY (NotificationId)
);
CREATE TABLE Translations (
    CommentId BIGINT NOT NULL,
    LanguageCode varchar(2) NOT NULL,
    TranslatedComment nvarchar(max),
    PRIMARY KEY (CommentId, LanguageCode)
);
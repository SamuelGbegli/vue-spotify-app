CREATE TABLE [dbo].[Users] (
    [ID]            UNIQUEIDENTIFIER NOT NULL,
    [SpotifyUserID] NVARCHAR (MAX)   NOT NULL,
    [DisplayName]   NVARCHAR (MAX)   NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([ID] ASC)
);


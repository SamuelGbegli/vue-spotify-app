CREATE TABLE [dbo].[SpotifyTokens] (
    [ID]            UNIQUEIDENTIFIER NOT NULL,
    [AccessToken]   NVARCHAR (MAX)   NOT NULL,
    [RefreshToken]  NVARCHAR (MAX)   NOT NULL,
    [ExpirationUTC] DATETIME2 (7)    NOT NULL,
    [TokenType]     NVARCHAR (MAX)   NOT NULL,
    [Scope]         NVARCHAR (MAX)   NOT NULL,
    CONSTRAINT [PK_SpotifyTokens] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_SpotifyTokens_Users_ID] FOREIGN KEY ([ID]) REFERENCES [dbo].[Users] ([ID]) ON DELETE CASCADE
);


CREATE TABLE [dbo].[PlaybackRecords] (
    [ID]         NVARCHAR (450) NOT NULL,
    [SpotifyID]  NVARCHAR (40)  NOT NULL,
    [DatePlayed] DATETIME2 (7)  NOT NULL,
    [Context]    NVARCHAR (16)  NULL,
    [ContextURI] NVARCHAR (64)  NULL,
    CONSTRAINT [PK_PlaybackRecords] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_PlaybackRecords_DatePlayed]
    ON [dbo].[PlaybackRecords]([DatePlayed] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_PlaybackRecords_SpotifyID_DatePlayed]
    ON [dbo].[PlaybackRecords]([SpotifyID] ASC, [DatePlayed] DESC);


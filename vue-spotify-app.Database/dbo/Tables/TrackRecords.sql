CREATE TABLE [dbo].[TrackRecords] (
    [ID]         NVARCHAR (450) NOT NULL,
    [SpotifyID]  NVARCHAR (40)  NULL,
    [DateAdded]  DATETIME2 (7)  NOT NULL,
    [PlaylistID] NVARCHAR (40)  NULL,
    [UserId]     NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_TrackRecords] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Tracks_SortName]
    ON [dbo].[TrackRecords]([ID] ASC);


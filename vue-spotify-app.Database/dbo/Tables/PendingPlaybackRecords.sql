CREATE TABLE [dbo].[PendingPlaybackRecords] (
    [ID]                NVARCHAR (450) NOT NULL,
    [DateRecorded]      DATETIME2 (7)  NOT NULL,
    [InputtedName]      NVARCHAR (MAX) NOT NULL,
    [InputtedSpotifyID] NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_PendingPlaybackRecords] PRIMARY KEY CLUSTERED ([ID] ASC)
);


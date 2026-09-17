CREATE TABLE [dbo].[Playlists] (
    [ID]             NVARCHAR (450) NOT NULL,
    [Name]           NVARCHAR (MAX) NOT NULL,
    [ImageURL]       NVARCHAR (MAX) NOT NULL,
    [NumberOfTracks] INT            NOT NULL,
    [OwnerName]      NVARCHAR (MAX) NOT NULL,
    [OwnerID]        NVARCHAR (MAX) NOT NULL,
    [SortName]       NVARCHAR (MAX) DEFAULT (N'') NOT NULL,
    [SnapshotID]     NVARCHAR (MAX) DEFAULT (N'') NOT NULL,
    CONSTRAINT [PK_Playlists] PRIMARY KEY CLUSTERED ([ID] ASC)
);


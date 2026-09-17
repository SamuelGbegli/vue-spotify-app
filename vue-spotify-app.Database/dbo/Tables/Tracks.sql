CREATE TABLE [dbo].[Tracks] (
    [ID]             NVARCHAR (450)   NOT NULL,
    [Name]           NVARCHAR (MAX)   NOT NULL,
    [AlbumID]        NVARCHAR (450)   NOT NULL,
    [SpotifyURI]     NVARCHAR (64)    NOT NULL,
    [ExternalURL]    NVARCHAR (256)   NOT NULL,
    [Length]         INT              NOT NULL,
    [Explicit]       BIT              NOT NULL,
    [SortName]       NVARCHAR (1024)  NOT NULL,
    [ISRC]           NVARCHAR (32)    NULL,
    [Playable]       BIT              NOT NULL,
    [MatchKey]       NVARCHAR (MAX)   NOT NULL,
    [AliasID]        UNIQUEIDENTIFIER NULL,
    [AlbumSortName]  NVARCHAR (1024)  NOT NULL,
    [ArtistSortName] NVARCHAR (1024)  NOT NULL,
    CONSTRAINT [PK_Tracks] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_Tracks_Albums_AlbumID] FOREIGN KEY ([AlbumID]) REFERENCES [dbo].[Albums] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Tracks_TrackAliases_AliasID] FOREIGN KEY ([AliasID]) REFERENCES [dbo].[TrackAliases] ([ID])
);


GO
CREATE NONCLUSTERED INDEX [IX_Tracks_AlbumID]
    ON [dbo].[Tracks]([AlbumID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tracks_AlbumSortName]
    ON [dbo].[Tracks]([AlbumSortName] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tracks_AlbumSortNameDesc]
    ON [dbo].[Tracks]([AlbumSortName] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_Tracks_AliasID]
    ON [dbo].[Tracks]([AliasID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tracks_ArtistSortName]
    ON [dbo].[Tracks]([ArtistSortName] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tracks_ArtistSortNameDesc]
    ON [dbo].[Tracks]([ArtistSortName] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_Tracks_Length]
    ON [dbo].[Tracks]([Length] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tracks_LengthDesc]
    ON [dbo].[Tracks]([Length] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_Tracks_SortName]
    ON [dbo].[Tracks]([ID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tracks_TrackSortName]
    ON [dbo].[Tracks]([SortName] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Tracks_TrackSortNameDesc]
    ON [dbo].[Tracks]([SortName] DESC);


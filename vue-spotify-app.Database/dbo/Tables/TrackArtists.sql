CREATE TABLE [dbo].[TrackArtists] (
    [ID]       UNIQUEIDENTIFIER NOT NULL,
    [TrackID]  NVARCHAR (450)   NOT NULL,
    [ArtistID] NVARCHAR (450)   NOT NULL,
    [Index]    INT              NOT NULL,
    CONSTRAINT [PK_TrackArtists] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_TrackArtists_Artists_ArtistID] FOREIGN KEY ([ArtistID]) REFERENCES [dbo].[Artists] ([ID]),
    CONSTRAINT [FK_TrackArtists_Tracks_TrackID] FOREIGN KEY ([TrackID]) REFERENCES [dbo].[Tracks] ([ID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TrackArtists_ArtistID]
    ON [dbo].[TrackArtists]([ArtistID] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TrackArtists_TrackID_ArtistID]
    ON [dbo].[TrackArtists]([TrackID] ASC, [ArtistID] ASC);


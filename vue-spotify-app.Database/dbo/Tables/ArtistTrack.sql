CREATE TABLE [dbo].[ArtistTrack] (
    [ID]       NVARCHAR (450) NOT NULL,
    [ArtistID] NVARCHAR (450) NOT NULL,
    [TrackID]  NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_ArtistTrack] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_ArtistTrack_Artists_ArtistID] FOREIGN KEY ([ArtistID]) REFERENCES [dbo].[Artists] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ArtistTrack_Tracks_TrackID] FOREIGN KEY ([TrackID]) REFERENCES [dbo].[Tracks] ([ID]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ArtistTrack_ArtistID_TrackID]
    ON [dbo].[ArtistTrack]([ArtistID] ASC, [TrackID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ArtistTrack_TrackID]
    ON [dbo].[ArtistTrack]([TrackID] ASC);


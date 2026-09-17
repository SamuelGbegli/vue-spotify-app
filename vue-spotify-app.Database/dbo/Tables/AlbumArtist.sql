CREATE TABLE [dbo].[AlbumArtist] (
    [ID]       NVARCHAR (450) NOT NULL,
    [AlbumID]  NVARCHAR (450) NOT NULL,
    [ArtistID] NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_AlbumArtist] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_AlbumArtist_Albums_AlbumID] FOREIGN KEY ([AlbumID]) REFERENCES [dbo].[Albums] ([ID]) ON DELETE CASCADE,
    CONSTRAINT [FK_AlbumArtist_Artists_ArtistID] FOREIGN KEY ([ArtistID]) REFERENCES [dbo].[Artists] ([ID]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AlbumArtist_AlbumID_ArtistID]
    ON [dbo].[AlbumArtist]([AlbumID] ASC, [ArtistID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_AlbumArtist_ArtistID]
    ON [dbo].[AlbumArtist]([ArtistID] ASC);


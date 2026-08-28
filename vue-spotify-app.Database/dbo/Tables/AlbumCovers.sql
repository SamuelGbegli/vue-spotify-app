CREATE TABLE [dbo].[AlbumCovers] (
    [ID]      NVARCHAR (450) NOT NULL,
    [Link]    NVARCHAR (MAX) NOT NULL,
    [Width]   INT            NOT NULL,
    [Height]  INT            NOT NULL,
    [AlbumID] NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_AlbumCovers] PRIMARY KEY CLUSTERED ([ID] ASC),
    CONSTRAINT [FK_AlbumCovers_Albums_AlbumID] FOREIGN KEY ([AlbumID]) REFERENCES [dbo].[Albums] ([ID]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AlbumCovers_AlbumID]
    ON [dbo].[AlbumCovers]([AlbumID] ASC);


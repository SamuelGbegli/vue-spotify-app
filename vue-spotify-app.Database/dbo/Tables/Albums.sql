CREATE TABLE [dbo].[Albums] (
    [ID]                   NVARCHAR (450) NOT NULL,
    [Name]                 NVARCHAR (MAX) NOT NULL,
    [NumberOfTracks]       INT            NOT NULL,
    [ReleaseDate]          NVARCHAR (16)  NOT NULL,
    [SpotifyURI]           NVARCHAR (64)  NOT NULL,
    [ExternalURL]          NVARCHAR (256) NOT NULL,
    [AlbumType]            NVARCHAR (32)  NOT NULL,
    [SortName]             NVARCHAR (MAX) NULL,
    [ReleaseDatePrecision] NVARCHAR (16)  NOT NULL,
    CONSTRAINT [PK_Albums] PRIMARY KEY CLUSTERED ([ID] ASC)
);


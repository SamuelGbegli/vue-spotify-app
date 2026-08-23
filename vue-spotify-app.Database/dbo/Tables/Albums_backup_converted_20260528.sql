CREATE TABLE [dbo].[Albums_backup_converted_20260528] (
    [ID]             NVARCHAR (450) NOT NULL,
    [Name]           NVARCHAR (MAX) NOT NULL,
    [NumberOfTracks] INT            NOT NULL,
    [ReleaseDate]    NVARCHAR (MAX) NOT NULL,
    [SpotifyURI]     NVARCHAR (MAX) NOT NULL,
    [ExternalURL]    NVARCHAR (MAX) NOT NULL,
    [SortName]       NVARCHAR (MAX) NULL,
    [AlbumType]      NVARCHAR (MAX) NOT NULL
);


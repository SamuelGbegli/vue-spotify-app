CREATE TABLE [dbo].[Artists] (
    [ID]          NVARCHAR (450) NOT NULL,
    [Name]        NVARCHAR (MAX) NOT NULL,
    [URI]         NVARCHAR (64)  NOT NULL,
    [ExternalURL] NVARCHAR (256) NOT NULL,
    [SortName]    NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_Artists] PRIMARY KEY CLUSTERED ([ID] ASC)
);


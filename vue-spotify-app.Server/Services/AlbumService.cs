using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Classes;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Services
{
    public class AlbumService
    {
        private readonly SpotifyAPIWrapper _spotifyAPIWrapper;
        private readonly DataContext _dataContext;

        public AlbumService(SpotifyAPIWrapper spotifyAPIWrapper, DataContext dataContext)
        {
            _spotifyAPIWrapper = spotifyAPIWrapper;
            _dataContext = dataContext;
        }

        public async Task<AlbumViewModel> GetAlbum(User user, string albumID)
        {
            var response = await _spotifyAPIWrapper.GetAsync<Classes.APIData.Album>(user.ID, $"/albums/{albumID}");
            var albumViewModel = new AlbumViewModel
            {
                ID = response.id,
                Name = response.name,
                Artists = response.artists.Select(a => new ArtistViewModel
                {
                    ID = a.id,
                    Name = a.name,
                    URI = a.uri,
                }).ToList(),
                NumberOfTracks = response.total_tracks,
                ReleaseDate = response.release_date,
                ImageURL = response.images != null && response.images.Count > 0 ? response.images.First().url : null,
                AlbumType = response.album_type,
                ExternalURL = response.external_urls.spotify
            };
            return albumViewModel;
        }

        public async Task<(AlbumViewModel, List<TrackViewModel>)> GetAlbumWithTracks(User user, string albumID)
        {
            var response = await _spotifyAPIWrapper.GetAsync<Classes.APIData.Album>(user.ID, $"/albums/{albumID}");
            var albumViewModel = new AlbumViewModel
            {
                ID = response.id,
                Name = response.name,
                Artists = response.artists.Select(a => new ArtistViewModel
                {
                    ID = a.id,
                    Name = a.name,
                    URI = a.uri,
                }).ToList(),
                NumberOfTracks = response.total_tracks,
                ReleaseDate = response.release_date,
                ImageURL = response.images != null && response.images.Count > 0 ? response.images.First().url : null,
                AlbumType = response.album_type,
                ExternalURL = response.external_urls.spotify
            };

            var trackViewModels = await GetAlbumTracks(user, albumID);
            return (albumViewModel, trackViewModels);
        }

        public async Task<List<TrackViewModel>> GetAlbumTracks(User user, string albumID, int offset = 0, int numberOfTracks = 50)
        {
            var userLikedSongsList = await _dataContext.TrackLists.FirstAsync(l => l.UserID == user.SpotifyUserID && l.TrackListType == TrackListType.LikedSongs);

            var lastPlayed =
                from p in _dataContext.PlaybackRecords
                join t in _dataContext.Tracks
                    on p.SpotifyID equals t.ID
                group p by t.AliasID into g
                select new
                {
                    AliasID = g.Key,
                    LastPlayed = g.Max(x => x.DatePlayed)
                };

            var likedTracks =
                from tr in _dataContext.TrackRecords
                join t in _dataContext.Tracks
                    on tr.SpotifyID equals t.ID
                where tr.TrackListID == userLikedSongsList.ID
                select t.AliasID;

            var response = await _spotifyAPIWrapper.GetAsync<Classes.APIData.TrackQueryResult>(user.ID, $"/albums/{albumID}/tracks?limit={numberOfTracks}&offset={offset}");

            var cachedAlbumTracks = _dataContext.Tracks
                .Where(t => response.items.Select(i => i.id).Contains(t.ID)).AsQueryable();

            var tracks = response.items.Select(i =>
            {
                var aliasID = _dataContext.Tracks.FirstOrDefault(t => t.ID == i.id)?.AliasID;
                return new TrackViewModel
                {
                    ID = i.id,
                    Name = i.name,
                    Artists = i.artists.Select(a => new ArtistViewModel
                    {
                        ID = a.id,
                        Name = a.name,
                        ExternalURL = a.external_urls.spotify,
                        URI = a.uri
                    }).ToList(),
                    Length = i.duration_ms,
                    URI = i.uri,
                    ExternalURL = i.external_urls.spotify,
                    IsInLikedSongs = likedTracks.Contains(aliasID),
                    DateLastPlayed = lastPlayed.FirstOrDefault(lp => lp.AliasID == aliasID)?.LastPlayed,
                    Index = offset + response.items.IndexOf(i) + 1
                };
            }).ToList();


            return tracks;
        }
    }
}

using vue_spotify_app.Classes;

namespace vue_spotify_app.Server.Services
{
    public class AlbumService
    {
        private readonly SpotifyAPIWrapper _spotifyAPIWrapper;

        public AlbumService(SpotifyAPIWrapper spotifyAPIWrapper)
        {
            _spotifyAPIWrapper = spotifyAPIWrapper;
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
                NumberOfTracks = response.total_tracks
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
                NumberOfTracks = response.total_tracks
            };

            var trackViewModels = response.tracks.items.Select(t => new TrackViewModel
            {
                ID = t.id,
                Name = t.name,
                Artists = t.artists.Select(a => new ArtistViewModel
                {
                    ID = a.id,
                    Name = a.name,
                    URI = a.uri
                }).ToList(),
                Length = t.duration_ms,
                URI = t.uri,
                ExternalURL = t.external_urls.spotify,
            }).ToList();
            return (albumViewModel, trackViewModels);
        }

        public async Task<List<TrackViewModel>> GetAlbumTracks(User user, string albumID, int offset = 1, int numberOfTracks = 50)
        {
            var response = await _spotifyAPIWrapper.GetAsync<Classes.APIData.TrackQueryResult>(user.ID, $"/albums/{albumID}/tracks?limit={numberOfTracks}&offset={offset}");
            var tracks = response.items.Select(t => new TrackViewModel
            {
                ID = t.id,
                Name = t.name,
                Artists = t.artists.Select(a => new ArtistViewModel
                {
                    ID = a.id,
                    Name = a.name,
                    URI = a.uri
                }).ToList(),
                Length = t.duration_ms,
                URI = t.uri,
                ExternalURL = t.external_urls.spotify,
            }).ToList();

            return tracks;
        }
    }
}

using System.Text;
using vue_spotify_app.Classes;
using vue_spotify_app.Classes.APIData;

namespace vue_spotify_app.Server.Services
{
    public class SearchService
    {
        private readonly SpotifyAPIWrapper _spotifyAPIWrapper;

        public SearchService(SpotifyAPIWrapper spotifyAPIWrapper)
        {
            _spotifyAPIWrapper = spotifyAPIWrapper;
        }

        public async Task<List<SearchResultViewModel>> Search(Guid userID, SearchDTO searchDTO)
        {
            var results = new List<SearchResultViewModel>();
            var tracks = new List<TrackViewModel>();
            var query = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(searchDTO.Query)) query.Append($"{searchDTO.Query} ");
            if (!string.IsNullOrWhiteSpace(searchDTO.Artist)) query.Append($"artist:{searchDTO.Artist} ");
            if (!string.IsNullOrWhiteSpace(searchDTO.Album)) query.Append($"album:{searchDTO.Album} ");
            if (!string.IsNullOrWhiteSpace(searchDTO.Track)) query.Append($"track:{searchDTO.Track} ");
            if (!string.IsNullOrWhiteSpace(searchDTO.Year)) query.Append($"year:{searchDTO.Year} ");
            if (!string.IsNullOrWhiteSpace(searchDTO.ISRC)) query.Append($"isrc:{searchDTO.ISRC} ");
            if (!string.IsNullOrWhiteSpace(searchDTO.Genre)) query.Append($"genre:{searchDTO.Genre} ");

            if (searchDTO.ItemTypes.Contains("album"))
            {
                if (string.IsNullOrWhiteSpace(searchDTO.UPC)) query.Append($"upc:{searchDTO.UPC} ");
                if (searchDTO.NewAlbums) query.Append("tag:new ");
                if (searchDTO.HipsterAlbums) query.Append("tag:hipster ");
            }
            if (query.Length > 0) query.Remove(query.Length - 1, 1);

            var types = searchDTO.ItemTypes.Any() ? string.Join(",", searchDTO.ItemTypes) : "album,artist,playlist,track,audiobook,show,episode";

            var response = await _spotifyAPIWrapper.GetAsync<SearchQueryResult>(userID, $"search?q={query}&type={types}&limit=10&offset={searchDTO.Offset}");

            if (searchDTO.ItemTypes.Contains("track") || searchDTO.ItemTypes.Count == 0)
            {
                var trackResults = new SearchResultViewModel
                {
                    ItemType = "track",
                    TotalItems = response.Tracks.total,
                    Page = searchDTO.Offset / 10 + 1,
                    Items = new List<object>()
                };

                foreach (var item in response.Tracks.items)
                {
                    var trackViewModel = new TrackViewModel
                    {
                        Name = item.name,
                        ExternalURL = item.external_urls.spotify,
                        AlbumName = item.album.name,
                        AlbumCover = item.album.images.FirstOrDefault(a => a.width == item.album.images.Max(i => i.width))?.url ?? "",
                        AlbumExternalURL = item.album.external_urls.spotify,
                        Length = item.duration_ms,
                        ID = item.id,
                    };
                    foreach (var artist in item.artists)
                    {
                        trackViewModel.Artists.Add(new ArtistViewModel
                        {
                            ID = artist.id,
                            Name = artist.name,
                            ExternalURL = artist.external_urls.spotify,
                            Index = item.artists.IndexOf(artist)
                        });
                    }

                   trackResults.Items.Add(trackViewModel);
                }
                results.Add(trackResults);
            }

            return results;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using vue_spotify_app.Classes;
using vue_spotify_app.Classes.APIData;
using vue_spotify_app.Classes.SortNameHelpers;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server
{
    public class PlaylistService
    {
        private readonly DataContext _dataContext;
        private readonly TrackService _trackService;
        private readonly SpotifyAPIWrapper _spotifyAPIWrapper;

        public PlaylistService(DataContext dataContext, TrackService trackService, SpotifyAPIWrapper spotifyAPIWrapper)
        {
            _dataContext = dataContext;
            _trackService = trackService;
            _spotifyAPIWrapper = spotifyAPIWrapper;
        }

        // TODO: remove
        public async Task<List<PlaylistViewModel>> GetPlaylists(string authToken, int offset, int numberOfPlaylists)
        {
            var viewModels = new List<PlaylistViewModel>();

            // Sets up HTTP Client
            HttpClient client = new HttpClient();
            // Adds JSON header to client
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            // Adds Spotify auth token to header
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Makes call to API

            // Gets ID of user
            HttpResponseMessage userMessage = client.GetAsync("https://api.spotify.com/v1/me").Result;
            string userId = "";
            if (userMessage.IsSuccessStatusCode)
            {
                using var contentStream = userMessage.Content.ReadAsStream();
                var user = System.Text.Json.JsonSerializer.Deserialize<Classes.APIData.UserProfile>(contentStream);
                userId = user.id;
            }

            HttpResponseMessage responseMessage = await client.GetAsync($"https://api.spotify.com/v1/me/playlists?offset={offset}&limit={numberOfPlaylists}");
            if (responseMessage.IsSuccessStatusCode)
            {
                using var contentStream = responseMessage.Content.ReadAsStream();

                // Deserialize JSON response to object
                var playlists = System.Text.Json.JsonSerializer.Deserialize<Classes.APIData.PlaylistResponse>(contentStream);

                foreach (var playlist in playlists.items)
                {
                    var viewModel = new PlaylistViewModel
                    {
                        ID = playlist.id,
                        Name = playlist.name,
                        Description = playlist.description,
                        NumberOfTracks = playlist.items.total,
                        OwnerName = playlist.owner.display_name,
                        OwnerLink = playlist.owner.external_urls.spotify,
                        ExternalURL = playlist.external_urls.spotify
                    };

                    if (playlist.images?.Any() == true && !playlist.images.IsNullOrEmpty())
                        viewModel.ImageLink = playlist.images.First(x => x.width == playlist.images.Max(y => y.width)).url;
                    viewModels.Add(viewModel);
                }
            }
            return viewModels;
        }

        // TODO: refactor to cache playlists in database and only call API to update playlists that have been modified since last fetch
        public async Task<(int, List<PlaylistViewModel>)> GetPlaylists(User user, int offset, int numberOfPlaylists, bool getUserEditablePlaylists = false)
        {
            var viewModels = new List<PlaylistViewModel>();

            // Makes call to API

            // Deserialize JSON response to object
            var playlists = getUserEditablePlaylists ?  _dataContext.Playlists.Where(p => p.OwnerID == user.SpotifyUserID).Skip(offset).Take(numberOfPlaylists).AsQueryable() :  _dataContext.Playlists.Skip(offset).Take(numberOfPlaylists).AsQueryable();
            var totalPlaylists = getUserEditablePlaylists ? await _dataContext.Playlists.CountAsync(p => p.OwnerID == user.SpotifyUserID) : await _dataContext.Playlists.CountAsync();

            foreach (var playlist in playlists)
            {
                    var viewModel = new PlaylistViewModel
                    {
                        ID = playlist.ID,
                        Name = playlist.Name,
                        NumberOfTracks = playlist.NumberOfTracks,
                        OwnerName = playlist.OwnerName,
//                        OwnerLink = playlist.,
                        ExternalURL = $"https://open.spotify.com/playlist/{playlist.ID}",
                        ImageLink = playlist.ImageURL
                    };
                viewModels.Add(viewModel);
            
        }
            return (totalPlaylists, viewModels);
        }

        // TODO: remove
        public async Task<int> GetNumberOfPlaylists(string authToken)
        {
            int numberOfPlaylists = 0;

            // Sets up HTTP Client
            HttpClient client = new HttpClient();
            // Adds JSON header to client
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            // Adds Spotify auth token to header
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // Makes call to API

            HttpResponseMessage responseMessage = await client.GetAsync("https://api.spotify.com/v1/me/playlists?");
            if (responseMessage.IsSuccessStatusCode)
            {
                using var contentStream = responseMessage.Content.ReadAsStream();

                // Deserialize JSON response to object
                var playlists = System.Text.Json.JsonSerializer.Deserialize<Classes.APIData.PlaylistResponse>(contentStream);

                numberOfPlaylists = playlists.total;
            }
            return numberOfPlaylists;
        }

        // TODO: Update to use cached playlist
        public async Task<PlaylistViewModel?> GetPlaylist(Guid userID, string playlistId)
        {
            var playlist = await _spotifyAPIWrapper.GetAsync<Classes.APIData.PlaylistItem>(userID, $"playlists/{playlistId}");
            // Creates view model with data fetched
            var viewModel = new PlaylistViewModel
            {
                ID = playlist.id,
                Name = playlist.name,
                Description = playlist.description,
                NumberOfTracks = playlist.items.total,
                OwnerName = playlist.owner.display_name,
                OwnerLink = playlist.owner.external_urls.spotify,
                ExternalURL = playlist.external_urls.spotify
            };

            // Sets playlist image if one exists
            if (playlist.images?.Any() == true && !playlist.images.IsNullOrEmpty())
                viewModel.ImageLink = playlist.images.First(x => x.width == playlist.images.Max(y => y.width)).url;

             return viewModel;
        }

        //TODO: Remove
        public async Task<List<TrackViewModel>> GetPlaylistTracks(
            string playlistId,
            string trackQuery = "",
            TrackSortType sortType = TrackSortType.Name,
            SortOrder sortOrder = SortOrder.Ascending,
            int offset = 0,
            int numberOfTracks = 10)
        {
            var tracks = new List<Classes.Track>();
            switch (sortType)
            {
                case TrackSortType.Name:
                    tracks = sortOrder == SortOrder.Ascending ?
                        await _dataContext.Tracks
                        .Include(tracks => tracks.Artists)
                        .Include(tracks => tracks.Album)
                        .ThenInclude(Album => Album.AlbumCover)
                        .Where(t => _dataContext.TrackRecords.Any(r => r.SpotifyID == t.ID && r.PlaylistID == playlistId) && t.Name.Contains(trackQuery))
                        .OrderBy(t => t.Name).Skip(offset).Take(numberOfTracks).ToListAsync() :
                         await _dataContext.Tracks
                        .Include(tracks => tracks.Artists)
                        .Include(tracks => tracks.Album)
                        .ThenInclude(Album => Album.AlbumCover)
                        .Where(t => _dataContext.TrackRecords.Any(r => r.SpotifyID == t.ID && r.PlaylistID == playlistId) && t.Name.Contains(trackQuery))
                        .OrderByDescending(t => t.Name).Skip(offset).Take(numberOfTracks).ToListAsync();
                    break;
                case TrackSortType.Artist:
                    tracks = sortOrder == SortOrder.Ascending ?
                        await _dataContext.Tracks
                        .Include(tracks => tracks.Artists)
                        .Include(tracks => tracks.Album)
                        .ThenInclude(Album => Album.AlbumCover)
                        .Where(t => _dataContext.TrackRecords.Any(r => r.SpotifyID == t.ID && r.PlaylistID == playlistId) && t.Name.Contains(trackQuery))
                        .OrderBy(t => t.Artists.FirstOrDefault().Name).Skip(offset).Take(numberOfTracks).ToListAsync() :
                         await _dataContext.Tracks
                        .Include(tracks => tracks.Artists)
                        .Include(tracks => tracks.Album)
                        .ThenInclude(Album => Album.AlbumCover)
                        .Where(t => _dataContext.TrackRecords.Any(r => r.SpotifyID == t.ID && r.PlaylistID == playlistId) && t.Name.Contains(trackQuery))
                        .OrderByDescending(t => t.Artists.FirstOrDefault().Name).Skip(offset).Take(numberOfTracks).ToListAsync();
                    break;
                case TrackSortType.Album:
                    tracks = sortOrder == SortOrder.Ascending ?
                        await _dataContext.Tracks
                        .Include(tracks => tracks.Artists)
                        .Include(tracks => tracks.Album)
                        .ThenInclude(Album => Album.AlbumCover)
                        .Where(t => _dataContext.TrackRecords.Any(r => r.SpotifyID == t.ID && r.PlaylistID == playlistId) && t.Name.Contains(trackQuery))
                        .OrderBy(t => t.Album.Name).Skip(offset).Take(numberOfTracks).ToListAsync() :
                         await _dataContext.Tracks
                        .Include(tracks => tracks.Artists)
                        .Include(tracks => tracks.Album)
                        .ThenInclude(Album => Album.AlbumCover)
                        .Where(t => _dataContext.TrackRecords.Any(r => r.SpotifyID == t.ID && r.PlaylistID == playlistId) && t.Name.Contains(trackQuery))
                        .OrderByDescending(t => t.Album.Name).Skip(offset).Take(numberOfTracks).ToListAsync(); ;
                    break;
                case TrackSortType.Duration:
                    tracks = sortOrder == SortOrder.Ascending ?
                        await _dataContext.Tracks
                        .Include(tracks => tracks.Artists)
                        .Include(tracks => tracks.Album)
                        .ThenInclude(Album => Album.AlbumCover)
                        .Where(t => _dataContext.TrackRecords.Any(r => r.SpotifyID == t.ID && r.PlaylistID == playlistId) && t.Name.Contains(trackQuery))
                        .OrderBy(t => t.Length).Skip(offset).Take(numberOfTracks).ToListAsync() :
                         await _dataContext.Tracks
                        .Include(tracks => tracks.Artists)
                        .Include(tracks => tracks.Album)
                        .ThenInclude(Album => Album.AlbumCover)
                        .Where(t => _dataContext.TrackRecords.Any(r => r.SpotifyID == t.ID && r.PlaylistID == playlistId) && t.Name.Contains(trackQuery))
                        .OrderByDescending(t => t.Length).Skip(offset).Take(numberOfTracks).ToListAsync();
                    break;
                case TrackSortType.DateAdded:

                    var trackIds = sortOrder == SortOrder.Ascending ?
                        await _dataContext.TrackRecords.Where(r => r.PlaylistID == playlistId).OrderBy(r => r.DateAdded).Select(r => r.SpotifyID).ToListAsync() :
                        await _dataContext.TrackRecords.Where(r => r.PlaylistID == playlistId).OrderByDescending(r => r.DateAdded).Select(r => r.SpotifyID).ToListAsync();
                    tracks = await _dataContext.Tracks
                        .Include(tracks => tracks.Artists)
                        .Include(tracks => tracks.Album)
                        .ThenInclude(Album => Album.AlbumCover)
                        .Where(t => trackIds.Contains(t.ID) && t.Name.Contains(trackQuery)).Skip(offset).Take(numberOfTracks)
                        .ToListAsync();
                    break;
                default:
                    break;
            }

            var trackViewModels = new List<TrackViewModel>();
            foreach (var track in tracks)
            {
                var trackViewModel = new TrackViewModel
                {
                    ID = track.ID,
                    Name = track.Name,
                    URI = track.SpotifyURI,
                    ExternalURL = track.ExternalURL,
                    AlbumName = track.Album?.Name ?? string.Empty,
                    AlbumCover = track.Album?.AlbumCover?.Link ?? string.Empty,
                    AlbumURI = track.Album?.SpotifyURI ?? string.Empty,
                    AlbumExternalURL = track.Album?.ExternalURL ?? string.Empty,
                    Length = track.Length,
                    DateSaved = _dataContext.TrackRecords
                        .FirstOrDefault(r => r.SpotifyID == track.ID && r.PlaylistID == playlistId)?.DateAdded,
                    IsInLikedSongs = await _dataContext.TrackRecords.AnyAsync(r => r.SpotifyID == track.ID && r.PlaylistID == null)
                };
                foreach (var artist in track.Artists)
                {
                    trackViewModel.Artists.Add(new Classes.ArtistViewModel
                    {
                        ID = artist.ID,
                        Name = artist.Name,
                        ExternalURL = artist.ExternalURL,
                        URI = artist.URI,
                        Index = track.Artists.IndexOf(artist)
                    });
                }

                // Looks for the last recorded time the track was played
                if (await _dataContext.PlaybackRecords.CountAsync(r => r.SpotifyID == track.ID) > 0)
                    trackViewModel.DateLastPlayed = _dataContext.PlaybackRecords.Where(r => r.SpotifyID == track.ID)
                            .OrderByDescending(r => r.DatePlayed).First().DatePlayed;

                trackViewModels.Add(trackViewModel);
            }
            return trackViewModels;
        }

        //// TODO: 
        //public async Task InitialisePlaylist(string authToken, int offset, int numberOfPlaylists)
        //{

        //    // Sets up HTTP Client
        //    HttpClient client = new HttpClient();
        //    // Adds JSON header to client
        //    client.DefaultRequestHeaders.Accept.Add(
        //        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        //    // Adds Spotify auth token to header
        //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

        //    // Makes call to API

        //    // Gets ID of user

        //    HttpStatusCode apiStatusCode = 0;
        //    string userId = "";
        //    while (apiStatusCode != HttpStatusCode.OK)
        //    {
        //        HttpResponseMessage userMessage = client.GetAsync("https://api.spotify.com/v1/me").Result;
        //        apiStatusCode = userMessage.StatusCode;
        //        if (userMessage.IsSuccessStatusCode)
        //        {
        //            using var contentStream = userMessage.Content.ReadAsStream();
        //            var user = System.Text.Json.JsonSerializer.Deserialize<Classes.APIData.UserProfile>(contentStream);
        //            userId = user.id;
        //        }
        //        else if (apiStatusCode == HttpStatusCode.TooManyRequests)
        //        {
        //            var retryAfter = userMessage.Headers.RetryAfter?.Delta?.TotalSeconds ?? 1;
        //            Thread.Sleep((int)(retryAfter * 1000));
        //        }
        //        else
        //        {
        //            throw new Exception("Could not get user details. Token is likely expired.");
        //        }
        //    }

        //    var apiUrl = $"https://api.spotify.com/v1/me/playlists?offset={offset}&limit={numberOfPlaylists}";

        //    while (apiUrl != null)
        //    {
        //        apiStatusCode = 0;
        //        while (apiStatusCode != HttpStatusCode.OK)
        //        {
        //            HttpResponseMessage responseMessage = await client.GetAsync(apiUrl);
        //            apiStatusCode = responseMessage.StatusCode;
        //            if (responseMessage.IsSuccessStatusCode)
        //            {
        //                using var contentStream = responseMessage.Content.ReadAsStream();

        //                // Deserialize JSON response to object
        //                var playlists = System.Text.Json.JsonSerializer.Deserialize<Classes.APIData.PlaylistResponse>(contentStream);

        //                foreach (var playlist in playlists.items)
        //                {
        //                    if (playlist.owner.id == userId)
        //                        await InitialisePlaylistTracks(playlist.id, authToken);
        //                }

        //                apiUrl = playlists.next;
        //            }
        //            else if(apiStatusCode == HttpStatusCode.TooManyRequests)
        //            {
        //                var retryAfter = responseMessage.Headers.RetryAfter?.Delta?.TotalSeconds ?? 1;
        //                Thread.Sleep((int)(retryAfter * 1000));
        //            }
        //            else
        //            {
        //                throw new Exception("Could not get playlists. Token is likely expired.");
        //            }
        //        }
        //    }

            
        //}

       public async Task InitialisePlaylistTracks(Guid userID)
        {
            const int bufferSize = 50;
            List<Classes.Track> tracksToAdd = new List<Classes.Track>(50);

            var user = await _dataContext.Users.FindAsync(userID);
            var playlists = await _dataContext.Playlists.Where(p => p.OwnerID == user.SpotifyUserID.ToString()).ToListAsync();
            foreach (var playlist in playlists)
            {
                var endpoint = $"playlists/{playlist.ID}/tracks?offset=0&limit=50";
                while(endpoint != null)
                {
                    var playlistRespone = await _spotifyAPIWrapper.GetAsync<LikedSongsPage>(userID, endpoint);
                    foreach(var item in playlistRespone.items)
                    {
                        if (!item.track.is_local)
                        {
                            var track = await _trackService.AddOrUpdateTrack(item.track);
                            if (await _dataContext.Tracks.FindAsync(track.ID) == null)
                            {
                                tracksToAdd.Add(track);
                            }
                            if (await _dataContext.TrackRecords.CountAsync(r =>
                            r.SpotifyID == item.track.id &&
                            r.PlaylistID == playlist.ID && r.DateAdded == DateTime.Parse(item.added_at)
                            ) == 0)
                            {
                                await _dataContext.TrackRecords.AddAsync(new TrackRecord
                                {
                                    UserId = item.added_by.id,
                                    SpotifyID = item.track.id,
                                    PlaylistID = playlist.ID,
                                    DateAdded = DateTime.Parse(item.added_at)
                                });
                                if (tracksToAdd.Count >= bufferSize)
                                {
                                    await _dataContext.Tracks.AddRangeAsync(tracksToAdd);
                                    await _dataContext.SaveChangesAsync();
                                    Debug.WriteLine($"Added ${tracksToAdd.Count} tracks from ${playlist.Name}");
                                    tracksToAdd.Clear();
                                }
                            }
                            else
                            {
                                await _dataContext.Tracks.AddRangeAsync(tracksToAdd);
                                tracksToAdd.Clear();
                                await _dataContext.SaveChangesAsync();
                                return;
                            }
                        }
                        endpoint = playlistRespone.next != null ? playlistRespone.next.Replace("https://api.spotify.com/v1/", "") : null;
                    }

                    if (tracksToAdd.Count > 0)
                    {
                        await _dataContext.Tracks.AddRangeAsync(tracksToAdd);
                        Debug.WriteLine($"Added ${tracksToAdd.Count} tracks from ${playlist.Name}");
                        tracksToAdd.Clear();
                    }
                    await _dataContext.SaveChangesAsync();
                }
            }
        }
   
        public async Task<int> GetNumberOfTrackPlaylists(string trackId)
        {
            return await _dataContext.TrackRecords.CountAsync(r => r.SpotifyID == trackId && r.PlaylistID != null);
        }

        public async Task<(int, List<TrackPlaylistViewModel>)> GetPlaylistsPerTrack(User user, string trackID, int offset = 0, int numberOfPlaylists = 1)
        {

            var totalFoundplaylists = await _dataContext.TrackRecords.CountAsync(r => r.SpotifyID == trackID && r.PlaylistID != null && r.UserId == user.SpotifyUserID);
            var playlists = new List<TrackPlaylistViewModel>();


            var items = await _dataContext.TrackRecords.Where(r => r.SpotifyID == trackID && r.PlaylistID != null && r.UserId == user.SpotifyUserID).Skip(offset).Take(numberOfPlaylists).ToListAsync();
            foreach(var item in items)
            {
                var playlist = await _dataContext.Playlists.FindAsync(item.PlaylistID);
                playlists.Add(new TrackPlaylistViewModel
                {
                    PlaylistID = playlist.ID,
                    PlaylistName = playlist.Name,
                    DateAdded = item.DateAdded,
                    // TODO: get playlist image from database
                    //Image = item.im
                });
            }

            return (totalFoundplaylists, playlists);
        }

        public async Task InitialisePlaylists(Guid userID)
        {
            const int bufferSize = 50;
            List<Playlist> playlistsToAdd = new List<Playlist>(50);
            var endpoint = $"me/playlists?offset=0&limit=50";
            while (endpoint != null)
            {
                var playlists = await _spotifyAPIWrapper.GetAsync<PlaylistResponse>(userID, endpoint);
                foreach (var playlist in playlists.items)
                {
                    var imageLink = playlist.images?.FirstOrDefault(x => x.width == playlist.images.Max(y => y.width))?.url ?? string.Empty;
                    Playlist? playlistEntity = await _dataContext.Playlists.FindAsync(playlist.id);
                    if (playlistEntity == null)
                    {
                        playlistEntity = new Playlist
                        {
                            ID = playlist.id,
                            Name = playlist.name,
                            SortName = RegexHelpers.GenerateSortName(playlist.name),
                            NumberOfTracks = playlist.items.total,
                            OwnerName = playlist.owner.display_name,
                            OwnerID = playlist.owner.id,
                            ImageURL = imageLink,
                            SnapshotID = playlist.snapshot_id
                        };
                        playlistsToAdd.Add(playlistEntity);
                    }
                    else if(playlistEntity.SnapshotID != playlist.snapshot_id)
                    {
                        playlistEntity.Name = playlist.name;
                        playlistEntity.SortName = RegexHelpers.GenerateSortName(playlist.name);
                        playlistEntity.NumberOfTracks = playlist.items.total;
                        playlistEntity.OwnerName = playlist.owner.display_name;
                        playlistEntity.OwnerID = playlist.owner.id;
                        playlistEntity.ImageURL = imageLink;
                        playlistEntity.SnapshotID = playlist.snapshot_id;
                    }

                    if (playlistsToAdd.Count == bufferSize)
                    {
                        _dataContext.Playlists.AddRange(playlistsToAdd);
                        await _dataContext.SaveChangesAsync();
                        Debug.WriteLine($"Added ${playlistsToAdd.Count} playlists");
                        playlistsToAdd.Clear();
                    }
                    endpoint = playlists.next != null ? playlists.next.Replace("https://api.spotify.com/v1/", "") : null;
                }
            }

            if (playlistsToAdd.Count > 0)
            {
                _dataContext.Playlists.AddRange(playlistsToAdd);
                Debug.WriteLine($"Added ${playlistsToAdd.Count} playlists");
                playlistsToAdd.Clear();
            }
            await _dataContext.SaveChangesAsync();
        }

        public async Task AddItemsToPlaylist(Guid UserID, string playlistID, List<string> trackIDs)
        {
            await _spotifyAPIWrapper.PostAsync(UserID, $"playlists/{playlistID}/tracks", new { uris = trackIDs.Select(id => $"spotify:track:{id}").ToList() });
        }
    }
}

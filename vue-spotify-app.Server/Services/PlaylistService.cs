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


        public async Task InitialisePlaylistTracks(Guid userID)
        {
            const int bufferSize = 50;
            List<Classes.Track> tracksToAdd = new List<Classes.Track>(50);
            List<TrackRecord> recordsToAdd = new List<TrackRecord>(50);

            var user = await _dataContext.Users.FindAsync(userID);
            var playlists = await _dataContext.Playlists.Where(p => p.OwnerID == user.SpotifyUserID.ToString()).ToListAsync();
            foreach (var playlist in playlists)
            {

                var playlistTrackList = await _dataContext.TrackLists.FirstOrDefaultAsync(l => l.TrackListType == TrackListType.Playlist && l.PlaylistID == playlist.ID);
                var endpoint = $"playlists/{playlist.ID}/tracks?offset=0&limit=50";
                var cutoffDate = await _dataContext.TrackRecords.Where(r => r.PlaylistID == playlist.ID).MaxAsync(r => r.DateAdded);
                while (endpoint != null)
                {
                    var playlistRespone = await _spotifyAPIWrapper.GetAsync<LikedSongsPage>(userID, endpoint);
                    var tracksAfterCutoff = playlistRespone.items.Where(i => DateTime.Parse(i.added_at) > cutoffDate);
                    var tracksBeforeCutoff = playlistRespone.items.Where(i => DateTime.Parse(i.added_at) <= cutoffDate);

                    foreach (var item in tracksAfterCutoff)
                    {
                        if (!item.track.is_local)
                        {
                            var track = await _trackService.AddOrUpdateTrack(item.track);
                            if (await _dataContext.Tracks.FindAsync(track.ID) == null)
                            {
                                tracksToAdd.Add(track);
                            }

                            if (tracksToAdd.Count >= bufferSize)
                            {
                                await _dataContext.Tracks.AddRangeAsync(tracksToAdd);
                                await _dataContext.SaveChangesAsync();
                                Debug.WriteLine($"Added ${tracksToAdd.Count} tracks from ${playlist.Name}");
                                tracksToAdd.Clear();
                            }

                            recordsToAdd.Add(new TrackRecord
                            {
                                UserId = item.added_by.id,
                                SpotifyID = item.track.id,
                                PlaylistID = playlist.ID,
                                DateAdded = DateTime.Parse(item.added_at)
                            });

                            if (recordsToAdd.Count >= bufferSize)
                            {
                                await _dataContext.TrackRecords.AddRangeAsync(recordsToAdd);
                                await _dataContext.SaveChangesAsync();
                                Debug.WriteLine($"Added ${recordsToAdd.Count} records from ${playlist.Name}");
                                tracksToAdd.Clear();
                            }
                        }
                        

                        

                    }
                    
                    if(tracksBeforeCutoff.Count() > 0)
                    {
                        var earliestTrackAdded = tracksBeforeCutoff.Min(t => DateTime.Parse(t.added_at));
                        var latestTrackAdded = tracksBeforeCutoff.Max(t => DateTime.Parse(t.added_at));

                        var tracksInTimeRange = await _dataContext.TrackRecords.Where(r =>
                            r.PlaylistID == playlist.ID &&
                            r.DateAdded >= earliestTrackAdded &&
                            r.DateAdded <= latestTrackAdded
                        ).ToListAsync();

                        foreach(var record in tracksInTimeRange)
                        {
                            if (!tracksBeforeCutoff.Select(t => t.track.id).Contains(record.ID))
                                _dataContext.TrackRecords.Remove(record);
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
                    else if (playlistEntity.SnapshotID != playlist.snapshot_id)
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
                        foreach(var item in playlistsToAdd)
                        {
                            var trackList = new TrackList
                            {
                                ID = Guid.NewGuid(),
                                Name = item.Name,
                                SortName = RegexHelpers.GenerateSortName(item.Name),
                                TrackListType = TrackListType.Playlist,
                                UserID = item.OwnerID,
                                PlaylistID = item.ID
                            };
                            await _dataContext.TrackLists.AddAsync(trackList);
                        }
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
                foreach (var item in playlistsToAdd)
                {
                    var trackList = new TrackList
                    {
                        ID = Guid.NewGuid(),
                        Name = item.Name,
                        SortName = RegexHelpers.GenerateSortName(item.Name),
                        TrackListType = TrackListType.Playlist,
                        UserID = item.OwnerID,
                        PlaylistID = item.ID
                    };
                    await _dataContext.TrackLists.AddAsync(trackList);
                }
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

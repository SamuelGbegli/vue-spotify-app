using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Classes;
using vue_spotify_app.Classes.APIData;
using vue_spotify_app.Classes.SortNameHelpers;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server
{
    public class TrackService
    {
        private readonly DataContext _dataContext;
        private readonly SpotifyAPIWrapper _spotifyAPIWrapper;

        public TrackService(DataContext dataContext, SpotifyAPIWrapper spotifyAPIWrapper)
        {
            _dataContext = dataContext;
            _spotifyAPIWrapper = spotifyAPIWrapper;
        }

        public async Task<(int, List<TrackViewModel>)> GetTracks(string spotifyUserID, TrackFilter filter, string? playlistId = null, int offset = 0, int numberOfTracks = 50)
        {

            var trackRecords = _dataContext.TrackRecords
                .Where(r => r.PlaylistID == playlistId && r.UserId == spotifyUserID)
                .Distinct();

            var lastPlayedQuery =
                from r in _dataContext.PlaybackRecords
                join t in _dataContext.Tracks
                    on r.SpotifyID equals t.ID
                group r by (t.AliasID) into g
                select new
                {
                    AliasId = g.Key,
                    LastPlayed = g.Max(x => x.DatePlayed),
                };

            var likedAliasQuery =
                from tr in _dataContext.TrackRecords
                join t in _dataContext.Tracks on tr.SpotifyID equals t.ID
                where tr.UserId == spotifyUserID
                && tr.PlaylistID == null
                select (t.AliasID);

            var likedAliasSet = likedAliasQuery.Distinct();

            // Sets up query to get tracks from playlist
            var trackQuery = _dataContext.Tracks
                .Where(t => trackRecords.Select(r => r.SpotifyID).Contains(t.ID))
                // .AsNoTracking()
                .Include(tracks => tracks.Artists)
                .Include(tracks => tracks.Album)
                .ThenInclude(Album => Album.AlbumCover)
                .AsQueryable();

            var query = from t in trackQuery
                        join tr in trackRecords
                            on t.ID equals tr.SpotifyID
                        join lp in lastPlayedQuery
                            on (t.AliasID) equals lp.AliasId into g
                        from lp in g.DefaultIfEmpty()
                        join liked in likedAliasSet
                            on (t.AliasID) equals liked into likedGroup
                         from liked in likedGroup.DefaultIfEmpty()
                        select new
                        {
                            Track = t,
                            LastPlayed = (DateTime?)lp.LastPlayed,
                            tr.DateAdded,
                            IsLiked = liked != null
                        };

            // Applies search filter to query
            if (!string.IsNullOrWhiteSpace(filter.Query))
            {
                // Removes whitespace from start and end of search query
                var search = filter.Query.Trim();

                // Value to ensure that search is applied to all fields if all options are true or false
                var searchAll = filter.SearchName == filter.SearchArtist && filter.SearchArtist == filter.SearchAlbum;

                // Filters query depending on selected fields
                query = query.Where(t =>
                    ((filter.SearchName || searchAll) && t.Track.Name.Contains(search)) ||
                    ((filter.SearchArtist || searchAll) && t.Track.Artists.Any(a => a.Name.Contains(search))) ||
                    ((filter.SearchAlbum || searchAll) && t.Track.Album.Name.Contains(search)));
            }

            if (filter.DateRangeFrom.HasValue && filter.DateRangeTo.HasValue)
            {
                // Ensures the earliest date is treated as the start of the range and the latest date is treated as the end of the range
                var dates = new List<DateTime> { filter.DateRangeFrom.Value, filter.DateRangeTo.Value };

                // Applies date range filter to query
                query = query.Where(t =>
                    t.DateAdded >= dates.Min() &&
                        t.DateAdded < dates.Max().AddDays(1));
            }
            else
            {
                // Gets tracks saved from a certain date if only a start date is provided
                if (filter.DateRangeFrom.HasValue)
                {
                    query = query.Where(t =>
                            t.DateAdded >= filter.DateRangeFrom.Value);
                }

                // Gets tracks saved up to a certain date if only an end date is provided
                if (filter.DateRangeTo.HasValue)
                {
                    query = query.Where(t =>
                        t.DateAdded <= filter.DateRangeTo.Value.AddDays(1));
                }
            }

            query = filter.SortType switch
            {
                SortType.Name => filter.SortOrder == SortOrder.Ascending
                ? query.OrderBy(t => t.Track.SortName)
                : query.OrderByDescending(t => t.Track.SortName),

                SortType.Artist => filter.SortOrder == SortOrder.Ascending
                ? query.OrderBy(t => t.Track.Artists.OrderBy(a => a.Name).Select(a => a.SortName).FirstOrDefault())
                : query.OrderByDescending(t => t.Track.Artists.OrderBy(a => a.Name).Select(a => a.SortName).FirstOrDefault()),

                SortType.Album => filter.SortOrder == SortOrder.Ascending ? query.OrderBy(t => t.Track.Album.SortName)
                : query.OrderByDescending(t => t.Track.Album.SortName),

                SortType.TrackLength => filter.SortOrder == SortOrder.Ascending ? query.OrderBy(t => t.Track.Length) : query.OrderByDescending(t => t.Track.Length),

                SortType.DateAdded => filter.SortOrder == SortOrder.Ascending ?
                    query.OrderBy(t => t.DateAdded) : query.OrderByDescending(t => t.DateAdded),

                SortType.DateLastPlayed => filter.SortOrder == SortOrder.Ascending ?
                query.OrderBy(t => t.LastPlayed) : query.OrderByDescending(t => t.LastPlayed),
                _ => query.OrderBy(t => t.Track.Name)
            };


            // Gets number of tracks satisfied by the query
            var total = await query.CountAsync();

            // Geta all tracks found in the query
            var tracks = await query
                .Skip(offset).Take(numberOfTracks).ToListAsync();

            var viewModels = new List<TrackViewModel>();

            // Converts track objects to view models
            foreach (var item in tracks)
            {
                var viewModel = new TrackViewModel
                {
                    ID = item.Track.ID,
                    Name = item.Track.Name,
                    URI = item.Track.SpotifyURI,
                    ExternalURL = item.Track.ExternalURL,
                    AlbumName = item.Track.Album.Name,
                    AlbumCover = item.Track.Album.AlbumCover.Link,
                    AlbumURI = item.Track.Album.SpotifyURI,
                    AlbumExternalURL = item.Track.Album.ExternalURL,
                    Length = item.Track.Length,
                    DateSaved = item.DateAdded,
                    Artists = item.Track.Artists.OrderBy(a => a.SortName).Select(a => new Classes.Artist
                    {
                        Name = a.Name,
                        ExternalURL = a.ExternalURL
                    }).ToList(),
                    DateLastPlayed = item.LastPlayed
                };
                if (!string.IsNullOrWhiteSpace(playlistId)) viewModel.IsInLikedSongs = item.IsLiked;
                viewModels.Add(viewModel);
            }

            // Returns number of tracks found and list of track view models
            return (total, viewModels);
        }

        /// <summary>
        /// Synchronizes the user's liked Spotify tracks with the local data context by retrieving track, album, and
        /// artist information from the Spotify Web API.
        /// </summary>
        /// <remarks>This method fetches all liked tracks for the authenticated user, including associated
        /// album and artist details, and updates the local data context accordingly. It processes tracks in pages,
        /// handling pagination automatically until all liked tracks are retrieved. The method may make multiple HTTP
        /// requests and can be subject to Spotify API rate limits. Ensure that the provided access token has the
        /// necessary scopes to access the user's saved tracks.</remarks>
        /// <param name="authToken">The OAuth access token used to authenticate requests to the Spotify Web API. Must be a valid token for the
        /// user whose tracks are to be synchronized.</param>
        /// <param name="apiUrl">The initial URL of the Spotify API endpoint to retrieve the user's liked tracks. Defaults to the first page
        /// of liked tracks if not specified.</param>
        /// <param name="singlePageOnly">If set to true, only the first page of liked tracks will be retrieved. Defaults to false.</param>
        public async Task InitialiseTracks(Guid userID, string? apiUrl = "me/tracks?limit=50", bool singlePageOnly = false, CancellationToken cancellationToken = default)
        {

            var user = await _dataContext.Users.Include(u => u.SpotifyToken).FirstOrDefaultAsync(u => u.ID == userID, cancellationToken);
            if (user == null || user.SpotifyToken == null) return;
            foreach (var track in await _dataContext.TrackRecords.Where(t => t.PlaylistID == null).ToListAsync())
            {


                var isInLikedSongs = await _spotifyAPIWrapper.GetAsync<bool[]>(user.ID, $"me/tracks/contains?ids={track.SpotifyID}");
                if (isInLikedSongs.Length > 0 && !isInLikedSongs[0])
                {
                    var trackRecord = await _dataContext.TrackRecords.FirstOrDefaultAsync(r => r.SpotifyID == track.ID && r.PlaylistID == null);
                    if (trackRecord != null)
                    {
                        _dataContext.TrackRecords.Remove(trackRecord);
                    }
                }

            }
            int offset = 0;

            while (apiUrl != null && !cancellationToken.IsCancellationRequested)
            {
                // Get liked songs page
                var likedSongs = await _spotifyAPIWrapper.GetAsync<LikedSongsPage>(user.ID, apiUrl);
                // Process each liked song
                foreach (var item in likedSongs.items)
                {
                    if (!item.track.is_local)
                    {
                        var track = await AddOrUpdateTrack(item.track);
                        /*// Gets or creates artist entities for the track
                        var trackArtists = new List<Classes.Artist>();
                        foreach (var artist in item.track.artists)
                        {
                            // Check if artist already exists in the database
                            Classes.Artist artistEntity;
                            // If artist is not in the database, create a new entity
                            if (!_dataContext.Artists.Any(a => a.ID == artist.id))
                            {
                                artistEntity = new Classes.Artist
                                {
                                    ID = artist.id,
                                    Name = artist.name,
                                    SortName = RegexHelpers.GenerateSortName(artist.name),
                                    URI = artist.uri,
                                    ExternalURL = artist.external_urls.spotify
                                };
                                _dataContext.Artists.Add(artistEntity);
                            }
                            // If artist exists, update details
                            else
                            {
                                artistEntity = _dataContext.Artists.First(a => a.ID == artist.id);
                                if (artistEntity.Name != artist.name)
                                {
                                    artistEntity.Name = artist.name;
                                    artistEntity.SortName = RegexHelpers.GenerateSortName(artist.name);
                                }
                                artistEntity.URI = artist.uri;
                                artistEntity.ExternalURL = artist.external_urls.spotify;
                                //_dataContext.SaveChanges();
                            }
                            // Add artist entity to track's artist list
                            trackArtists.Add(artistEntity);
                        }
                        // Gets or creates album entity for the track
                        var albumArtists = new List<Classes.Artist>();
                        foreach (var artist in item.track.album.artists)
                        {
                            Classes.Artist artistEntity;
                            // Check if artist is in the track artists list first
                            if (trackArtists.Any(a => a.ID == artist.id))
                            {
                                artistEntity = trackArtists.First(a => a.ID == artist.id);
                            }
                            else if (!_dataContext.Artists.Any(a => a.ID == artist.id))
                            {
                                artistEntity = new Classes.Artist
                                {
                                    ID = artist.id,
                                    Name = artist.name,
                                    SortName = RegexHelpers.GenerateSortName(artist.name),
                                    URI = artist.uri,
                                    ExternalURL = artist.external_urls.spotify
                                };
                                _dataContext.Artists.Add(artistEntity);
                                //_dataContext.SaveChanges();
                            }
                            else
                            {
                                artistEntity = _dataContext.Artists.First(a => a.ID == artist.id);
                                if (artistEntity.Name != artist.name)
                                {
                                    artistEntity.Name = artist.name;
                                    artistEntity.SortName = RegexHelpers.GenerateSortName(artist.name);
                                }
                                artistEntity.URI = artist.uri;
                                artistEntity.ExternalURL = artist.external_urls.spotify;
                            }
                            albumArtists.Add(artistEntity);
                        }
                        // Check if album exists in the database
                        var album = item.track.album;
                        Classes.Album albumEntity;
                        if (album != null && !_dataContext.Albums.Any(a => a.ID == album.id))
                        {
                            albumEntity = new Classes.Album
                            {
                                ID = album.id,
                                Name = album.name,
                                SortName = RegexHelpers.GenerateSortName(album.name),
                                ReleaseDate = album.release_date,
                                NumberOfTracks = album.total_tracks,
                                Artists = albumArtists,
                                SpotifyURI = album.uri,
                                ExternalURL = album.external_urls.spotify
                            };
                            albumEntity.AlbumCover = new Classes.AlbumCover
                            {
                                Height = (int)album.images[0].height,
                                Width = (int)album.images[0].width,
                                Link = album.images[0].url
                            };
                        }
                        else
                        {
                            albumEntity = _dataContext.Albums.First(a => a.ID == album.id);
                            if (albumEntity.Name != album.name)
                            {
                                albumEntity.Name = album.name;
                                albumEntity.SortName = RegexHelpers.GenerateSortName(album.name);
                            }
                            albumEntity.ReleaseDate = album.release_date;
                            albumEntity.NumberOfTracks = album.total_tracks;
                            albumEntity.SpotifyURI = album.uri;
                            albumEntity.ExternalURL = album.external_urls.spotify;
                        }
                        // Check if track exists in the database
                        var track = item.track;
                        if (!_dataContext.Tracks.Any(t => t.ID == track.id))
                        {
                            var newTrack = new Classes.Track
                            {
                                ID = track.id,
                                Name = track.name,
                                SortName = RegexHelpers.GenerateSortName(track.name),
                                AlbumID = track.album.id,
                                Album = albumEntity,
                                Artists = trackArtists,
                                SpotifyURI = track.uri,
                                ExternalURL = track.external_urls.spotify,
                                Length = track.duration_ms,
                                Explicit = track.@explicit,
                                ISRC = track.external_ids?.isrc
                            };
                            _dataContext.Tracks.Add(newTrack);
                        }
                        else
                        {
                            var existingTrack = _dataContext.Tracks.Include(t => t.Artists).First(t => t.ID == track.id);
                            if (existingTrack.Name != track.name)
                            {
                                existingTrack.Name = track.name;
                                existingTrack.SortName = RegexHelpers.GenerateSortName(track.name);
                            }
                            existingTrack.AlbumID = track.album.id;
                            existingTrack.SpotifyURI = track.uri;
                            existingTrack.ExternalURL = track.external_urls.spotify;
                            existingTrack.Length = track.duration_ms;
                            existingTrack.Explicit = track.@explicit;
                            existingTrack.Album = _dataContext.Albums.First(a => a.ID == track.album.id);
                            existingTrack.ISRC = track.external_ids?.isrc;

                            foreach (var artist in trackArtists)
                            {
                                if (!existingTrack.Artists.Any(a => a.ID == artist.ID))
                                    existingTrack.Artists.Add(artist);
                            }

                            var artistsToRemove = existingTrack.Artists.Where(a => !trackArtists.Any(ta => ta.ID == a.ID)).ToList();
                            foreach (var r in artistsToRemove) existingTrack.Artists.Remove(r);
                        }
                       */
                        if (await _dataContext.Tracks.FindAsync(track.ID) == null)
                        {
                            await _dataContext.Tracks.AddAsync(track);
                        }
                        if (!await _dataContext.TrackRecords.AnyAsync(t => t.UserId == user.SpotifyUserID && t.SpotifyID == track.ID && t.PlaylistID == null))
                        {
                            var trackRecord = new TrackRecord
                            {
                                UserId = user.SpotifyUserID,
                                SpotifyID = track.ID,
                                DateAdded = DateTime.Parse(item.added_at)
                            };
                            await _dataContext.TrackRecords.AddAsync(trackRecord);
                        }
                    }
                }
                // If there are no more pages, exit the loop
                if (likedSongs.next == null)
                {
                    apiUrl = null;
                }
                else
                {
                    offset += likedSongs.limit;
                    apiUrl = $"me/tracks?limit={likedSongs.limit}&offset={offset}";
                }

                await _dataContext.SaveChangesAsync(cancellationToken);
                //Handle pagination
                // If only a single page is to be processed, break the loop
                if (singlePageOnly)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Returns the number of tracks in the user's Liked Songs library.
        /// </summary>
        /// <param name="trackQuery">A string all tracks must include to be returned.</param>
        /// <returns>The number of tracks that match the srting provided.</returns>
        public int GetNumberOfTracks(string trackQuery)
        {
            return _dataContext.Tracks.Count(t => t.Name.Contains(trackQuery) && _dataContext.TrackRecords.Any(r => r.SpotifyID == t.ID && r.PlaylistID == null));
        }

        /// <summary>
        /// Returns information regarding a track in Spotify.
        /// </summary>
        /// <param name="id">Ihe inputted track ID.</param>
        /// <param name="authToken">The authorisation token used to view track information.</param>
        /// <returns>A view model containing information about a track.</returns>
        /// <exception cref="Exception"></exception>
        public async Task<TrackViewModel?> GetTrack(Guid userID, string trackID)
        {
            try
            {
                var track = await _spotifyAPIWrapper.GetAsync<Classes.APIData.Track>(userID, $"tracks/{trackID}");

                var viewModel = new TrackViewModel
                {
                    Name = track.name,
                    ExternalURL = track.external_urls.spotify,
                    AlbumName = track.album.name,
                    AlbumCover = track.album.images.FirstOrDefault(a => a.width == track.album.images.Max(i => i.width)).url,
                    AlbumExternalURL = track.album.external_urls.spotify,
                    Length = track.duration_ms,
                    ID = trackID,
                };
                foreach (var artist in track.artists)
                {
                    viewModel.Artists.Add(new Classes.Artist
                    {
                        Name = artist.name,
                        ExternalURL = artist.external_urls.spotify,
                    });
                }
                var likedSongTrack = await _dataContext.TrackRecords.FirstOrDefaultAsync(x => x.SpotifyID == trackID && x.PlaylistID == null);
                if (likedSongTrack != null)
                {
                    viewModel.DateSaved = likedSongTrack.DateAdded;
                }
                return viewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving track information: {ex.Message}");
                return null;
            }
        }

        public async Task<Classes.Track> AddOrUpdateTrack(Classes.APIData.Track track)
        {
            // Gets or creates artist entities for the track
            var trackArtists = new List<Classes.Artist>();
            foreach (var artist in track.artists)
            {
                // Check if artist already exists in the database
                Classes.Artist artistEntity;
                // If artist is not in the database, create a new entity
                if (!_dataContext.Artists.Any(a => a.ID == artist.id))
                {
                    artistEntity = new Classes.Artist
                    {
                        ID = artist.id,
                        Name = artist.name,
                        SortName = RegexHelpers.GenerateSortName(artist.name),
                        URI = artist.uri,
                        ExternalURL = artist.external_urls.spotify
                    };
                    await _dataContext.Artists.AddAsync(artistEntity);
                }
                // If artist exists, update details
                else
                {
                    artistEntity = _dataContext.Artists.First(a => a.ID == artist.id);
                    if (artistEntity.Name != artist.name)
                    {
                        artistEntity.Name = artist.name;
                        artistEntity.SortName = RegexHelpers.GenerateSortName(artist.name);
                    }
                    artistEntity.URI = artist.uri;
                    artistEntity.ExternalURL = artist.external_urls.spotify;
                    //_dataContext.SaveChanges();
                }
                // Add artist entity to track's artist list
                trackArtists.Add(artistEntity);
            }
            // Gets or creates album entity for the track
            var albumArtists = new List<Classes.Artist>();
            foreach (var artist in track.album.artists)
            {
                Classes.Artist artistEntity;
                // Check if artist is in the track artists list first
                if (trackArtists.Any(a => a.ID == artist.id))
                {
                    artistEntity = trackArtists.First(a => a.ID == artist.id);
                }
                else if (!_dataContext.Artists.Any(a => a.ID == artist.id))
                {
                    artistEntity = new Classes.Artist
                    {
                        ID = artist.id,
                        Name = artist.name,
                        SortName = RegexHelpers.GenerateSortName(artist.name),
                        URI = artist.uri,
                        ExternalURL = artist.external_urls.spotify
                    };
                    _dataContext.Artists.Add(artistEntity);
                    //_dataContext.SaveChanges();
                }
                else
                {
                    artistEntity = _dataContext.Artists.First(a => a.ID == artist.id);
                    if (artistEntity.Name != artist.name)
                    {
                        artistEntity.Name = artist.name;
                        artistEntity.SortName = RegexHelpers.GenerateSortName(artist.name);
                    }
                    artistEntity.URI = artist.uri;
                    artistEntity.ExternalURL = artist.external_urls.spotify;
                }
                albumArtists.Add(artistEntity);
            }
            // Check if album exists in the database
            var album = track.album;
            Classes.Album albumEntity;
            if (album != null && !_dataContext.Albums.Any(a => a.ID == album.id))
            {
                albumEntity = new Classes.Album
                {
                    ID = album.id,
                    Name = album.name,
                    SortName = RegexHelpers.GenerateSortName(album.name),
                    ReleaseDate = album.release_date,
                    NumberOfTracks = album.total_tracks,
                    Artists = albumArtists,
                    SpotifyURI = album.uri,
                    ExternalURL = album.external_urls.spotify,
                    AlbumType = album.type
                };
                albumEntity.AlbumCover = new Classes.AlbumCover
                {
                    Height = (int)album.images[0].height,
                    Width = (int)album.images[0].width,
                    Link = album.images[0].url
                };
                await _dataContext.Albums.AddAsync(albumEntity);

            }
            else
            {
                albumEntity = _dataContext.Albums.First(a => a.ID == album.id);
                if (albumEntity.Name != album.name)
                {
                    albumEntity.Name = album.name;
                    albumEntity.SortName = RegexHelpers.GenerateSortName(album.name);
                }
                albumEntity.ReleaseDate = album.release_date;
                albumEntity.NumberOfTracks = album.total_tracks;
                albumEntity.SpotifyURI = album.uri;
                albumEntity.ExternalURL = album.external_urls.spotify;
                albumEntity.AlbumType = album.album_type;
            }
            await _dataContext.SaveChangesAsync();
            Classes.Track trackEntity;
            // Check if track exists in the database
            if (!_dataContext.Tracks.Any(t => t.ID == track.id))
            {
                trackEntity = new Classes.Track
                {
                    ID = track.id,
                    Name = track.name,
                    SortName = RegexHelpers.GenerateSortName(track.name),
                    AlbumID = track.album.id,
                    Album = albumEntity,
                    Artists = trackArtists,
                    SpotifyURI = track.uri,
                    ExternalURL = track.external_urls.spotify,
                    Length = track.duration_ms,
                    Explicit = track.@explicit,
                    ISRC = track.external_ids?.isrc,
                    Playable = track.is_playable
                };
            }
            else
            {
                trackEntity = _dataContext.Tracks.Include(t => t.Artists).First(t => t.ID == track.id);
                if (trackEntity.Name != track.name)
                {
                    trackEntity.Name = track.name;
                    trackEntity.SortName = RegexHelpers.GenerateSortName(track.name);
                }
                trackEntity.AlbumID = track.album.id;
                trackEntity.SpotifyURI = track.uri;
                trackEntity.ExternalURL = track.external_urls.spotify;
                trackEntity.Length = track.duration_ms;
                trackEntity.Explicit = track.@explicit;
                trackEntity.Album = _dataContext.Albums.First(a => a.ID == track.album.id);
                trackEntity.ISRC = track.external_ids?.isrc;
                trackEntity.Playable = track.is_playable;

                foreach (var artist in trackArtists)
                {
                    if (!trackEntity.Artists.Any(a => a.ID == artist.ID))
                        trackEntity.Artists.Add(artist);
                }

                var artistsToRemove = trackEntity.Artists.Where(a => !trackArtists.Any(ta => ta.ID == a.ID)).ToList();
                foreach (var r in artistsToRemove) trackEntity.Artists.Remove(r);
            }
            trackEntity.GenerateMatchKey();
            return trackEntity;
        }

    }
}


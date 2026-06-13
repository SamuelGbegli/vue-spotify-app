using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Diagnostics;
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

        //public async Task<(int, List<TrackViewModelBatch>)> GetTracks(
        //    string spotifyUserID,
        //    TrackFilter filter,
        //    List<int> offsets,
        //    string? playlistId = null,
        //    int numberOfTracks = 50
        //    )
        //{
        //    var batches = new List<TrackViewModelBatch>();

        //    if (offsets == null) offsets = new List<int> { 0 };

        //    var trackRecords = _dataContext.TrackRecords
        //        .Where(r => r.PlaylistID == playlistId && r.UserId == spotifyUserID)
        //        .Distinct();

        //    var lastPlayedQuery =
        //        from r in _dataContext.PlaybackRecords
        //        join t in _dataContext.Tracks
        //            on r.SpotifyID equals t.ID
        //        group r by (t.AliasID) into g
        //        select new
        //        {
        //            AliasId = g.Key,
        //            LastPlayed = g.Max(x => x.DatePlayed),
        //        };

        //    var likedAliasQuery =
        //        from tr in _dataContext.TrackRecords
        //        join t in _dataContext.Tracks on tr.SpotifyID equals t.ID
        //        where tr.UserId == spotifyUserID
        //        && tr.PlaylistID == null
        //        select (t.AliasID);

        //    var likedAliasSet = likedAliasQuery.Distinct();

        //    // Sets up query to get tracks from playlist
        //    var trackQuery = _dataContext.Tracks
        //        .Where(t => trackRecords.Select(r => r.SpotifyID).Contains(t.ID))
        //        // .AsNoTracking()
        //        .Include(tracks => tracks.Artists)
        //        .Include(tracks => tracks.Album)
        //        .ThenInclude(Album => Album.AlbumCover)
        //        .AsQueryable();

        //    var query = from t in trackQuery
        //                join tr in trackRecords
        //                    on t.ID equals tr.SpotifyID
        //                join lp in lastPlayedQuery
        //                    on (t.AliasID) equals lp.AliasId into g
        //                from lp in g.DefaultIfEmpty()
        //                join liked in likedAliasSet
        //                    on (t.AliasID) equals liked into likedGroup
        //                from liked in likedGroup.DefaultIfEmpty()
        //                select new
        //                {
        //                    Track = t,
        //                    LastPlayed = (DateTime?)lp.LastPlayed,
        //                    tr.DateAdded,
        //                    IsLiked = liked != null
        //                };

        //    // Applies search filter to query
        //    if (!string.IsNullOrWhiteSpace(filter.Query))
        //    {
        //        // Removes whitespace from start and end of search query
        //        var search = filter.Query.Trim();

        //        // Value to ensure that search is applied to all fields if all options are true or false
        //        var searchAll = filter.SearchName == filter.SearchArtist && filter.SearchArtist == filter.SearchAlbum;

        //        // Filters query depending on selected fields
        //        query = query.Where(t =>
        //            ((filter.SearchName || searchAll) && t.Track.Name.Contains(search)) ||
        //            ((filter.SearchArtist || searchAll) && t.Track.Artists.Any(a => a.Name.Contains(search))) ||
        //            ((filter.SearchAlbum || searchAll) && t.Track.Album.Name.Contains(search)));
        //    }

        //    if (filter.DateRangeFrom.HasValue && filter.DateRangeTo.HasValue)
        //    {
        //        // Ensures the earliest date is treated as the start of the range and the latest date is treated as the end of the range
        //        var dates = new List<DateTime> { filter.DateRangeFrom.Value, filter.DateRangeTo.Value };

        //        // Applies date range filter to query
        //        query = query.Where(t =>
        //            t.DateAdded >= dates.Min() &&
        //                t.DateAdded < dates.Max().AddDays(1));
        //    }
        //    else
        //    {
        //        // Gets tracks saved from a certain date if only a start date is provided
        //        if (filter.DateRangeFrom.HasValue)
        //        {
        //            query = query.Where(t =>
        //                    t.DateAdded >= filter.DateRangeFrom.Value);
        //        }

        //        // Gets tracks saved up to a certain date if only an end date is provided
        //        if (filter.DateRangeTo.HasValue)
        //        {
        //            query = query.Where(t =>
        //                t.DateAdded <= filter.DateRangeTo.Value.AddDays(1));
        //        }
        //    }

        //    query = filter.SortType switch
        //    {
        //        SortType.Name => filter.SortOrder == SortOrder.Ascending
        //        ? query.OrderBy(t => t.Track.SortName)
        //        : query.OrderByDescending(t => t.Track.SortName),

        //        SortType.Artist => filter.SortOrder == SortOrder.Ascending
        //        ? query.OrderBy(t => t.Track.Artists.OrderBy(a => a.Name).Select(a => a.SortName).FirstOrDefault())
        //        : query.OrderByDescending(t => t.Track.Artists.OrderBy(a => a.Name).Select(a => a.SortName).FirstOrDefault()),

        //        SortType.Album => filter.SortOrder == SortOrder.Ascending ? query.OrderBy(t => t.Track.Album.SortName)
        //        : query.OrderByDescending(t => t.Track.Album.SortName),

        //        SortType.TrackLength => filter.SortOrder == SortOrder.Ascending ? query.OrderBy(t => t.Track.Length) : query.OrderByDescending(t => t.Track.Length),

        //        SortType.DateAdded => filter.SortOrder == SortOrder.Ascending ?
        //            query.OrderBy(t => t.DateAdded) : query.OrderByDescending(t => t.DateAdded),

        //        SortType.DateLastPlayed => filter.SortOrder == SortOrder.Ascending ?
        //        query.OrderBy(t => t.LastPlayed) : query.OrderByDescending(t => t.LastPlayed),
        //        _ => query.OrderBy(t => t.Track.Name)
        //    };


        //    // Gets number of tracks satisfied by the query
        //    var total = await query.CountAsync();

        //    // Geta all tracks found in the query
        //    foreach (var index in offsets)
        //    {
        //        var sql = query
        //            .AsNoTracking()
        //            .AsSplitQuery()
        //            .ToQueryString();
        //        Console.WriteLine(sql);

        //        var tracks = await query
        //            .AsNoTracking()
        //            .AsSplitQuery()
        //        .Skip(index).Take(numberOfTracks).ToListAsync();

        //        var trackBatch = new TrackViewModelBatch
        //        {
        //            BatchIndex = (int)Math.Ceiling((double)index / (double)numberOfTracks) + 1,
        //            TrackViewModels = tracks.Select(t => new TrackViewModel
        //            {
        //                ID = t.Track.ID,
        //                Name = t.Track.Name,
        //                URI = t.Track.SpotifyURI,
        //                ExternalURL = t.Track.ExternalURL,
        //                AlbumName = t.Track.Album.Name,
        //                AlbumCover = t.Track.Album.AlbumCover.Link,
        //                AlbumURI = t.Track.Album.SpotifyURI,
        //                AlbumExternalURL = t.Track.Album.ExternalURL,
        //                Length = t.Track.Length,
        //                DateSaved = t.DateAdded,
        //                Artists = t.Track.Artists.OrderBy(a => a.SortName).Select(a => new Classes.Artist
        //                {
        //                    Name = a.Name,
        //                    ExternalURL = a.ExternalURL
        //                }).ToList(),
        //                DateLastPlayed = t.LastPlayed,
        //                IsInLikedSongs = t.IsLiked
        //            }).ToList()
        //        };
        //        batches.Add(trackBatch);
        //    }

        //    // Returns number of tracks found and list of track view models
        //    return (total, batches);
        //}

        public async Task<(int, List<TrackViewModelBatch>)> GetTracksNew(string spotifyUserID, TrackFilter filter, List<int> offsets, string? playlistId = null, int numberOfTracks = 50)
        {
            var query =
                from tr in _dataContext.TrackRecords
                join t in _dataContext.Tracks
                    on tr.SpotifyID equals t.ID
                where tr.UserId == spotifyUserID
                    && tr.PlaylistID == playlistId
                select new
                {
                    TrackID = t.ID,
                    t.Name,
                    t.SortName,
                    t.ArtistSortName,
                    t.AlbumSortName,
                    t.Length,
                    t.AliasID,
                    tr.DateAdded
                };

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
                where tr.UserId == spotifyUserID
                    && tr.PlaylistID == null
                select t.AliasID;

            var batches = new List<TrackViewModelBatch>();

            // Applies search filter to query
            if (!string.IsNullOrWhiteSpace(filter.Query))
            {
                // Removes whitespace from start and end of search query
                var search = filter.Query.Trim();

                // Value to ensure that search is applied to all fields if all options are true or false
                var searchAll = filter.SearchName == filter.SearchArtist && filter.SearchArtist == filter.SearchAlbum;

                // Filters query depending on selected fields
                query = query.Where(t =>
                    ((filter.SearchName || searchAll) && t.Name.Contains(search))); //||
                                                                                    //((filter.SearchArtist || searchAll) && t.Artists.Any(a => a.Name.Contains(search))) ||
                                                                                    // ((filter.SearchAlbum || searchAll) && t.Album.Name.Contains(search)));
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

            switch (filter.SortType)
            {
                case SortType.Name:
                    query = filter.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(t => t.SortName)
                    : query.OrderByDescending(t => t.SortName);
                    break;

                case SortType.Artist:
                    query = filter.SortOrder == SortOrder.Ascending
                    ? query.OrderBy(t => t.ArtistSortName)
                    : query.OrderByDescending(t => t.ArtistSortName);
                    break;

                case SortType.Album:
                    query = filter.SortOrder == SortOrder.Ascending ? query.OrderBy(t => t.AlbumSortName)
                    : query.OrderByDescending(t => t.AlbumSortName);
                    break;

                case SortType.TrackLength:
                    query = filter.SortOrder == SortOrder.Ascending ? query.OrderBy(t => t.Length) : query.OrderByDescending(t => t.Length);
                    break;

                case SortType.DateAdded:
                    query = filter.SortOrder == SortOrder.Ascending ?
                    query.OrderBy(t => t.DateAdded) : query.OrderByDescending(t => t.DateAdded);
                    break;


                case SortType.DateLastPlayed:
                    query = filter.SortOrder == SortOrder.Ascending ?
                    from t in query
                    join lp in lastPlayed
                        on t.AliasID equals lp.AliasID into g
                    from lp in g.DefaultIfEmpty()
                    orderby lp.LastPlayed
                    select t
                    :
                    from t in query
                    join lp in lastPlayed
                        on t.AliasID equals lp.AliasID into g
                    from lp in g.DefaultIfEmpty()
                    orderby lp.LastPlayed descending
                    select t;
                    break;
            }
            ;

            var totalItems = await query.Select(t => t.TrackID).CountAsync();

            foreach (var offset in offsets)
            {
                var batch = new TrackViewModelBatch
                {
                    BatchIndex = (int)Math.Ceiling((double)offset / (double)numberOfTracks) + 1
                };


                var items = await query
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Skip(offset)
                    .Take(numberOfTracks)
                    .ToListAsync();
                var ids = items.Select(i => i.TrackID);

                var tracks = _dataContext.Tracks
                    .Where(t => ids.Contains(t.ID))
                    .Include(t => t.Artists)
                    .Include(t => t.TrackArtists)
                    .Include(t => t.Album)
                        .ThenInclude(t => t.AlbumCover)
                    .AsNoTracking()
                    .AsSplitQuery()
                    .ToList();


                var trackViewModels = from i in items
                                      join t in tracks on i.TrackID equals t.ID
                                      join liked in likedTracks on i.AliasID equals liked into likedGroup
                                      from liked in likedGroup.DefaultIfEmpty()
                                      select new TrackViewModel
                                      {
                                          ID = i.TrackID,
                                          Name = i.Name,
                                          URI = t.SpotifyURI,
                                          ExternalURL = t.ExternalURL,
                                          AlbumName = t.Album.Name,
                                          AlbumCover = t.Album.AlbumCover.Link,
                                          AlbumURI = t.Album.SpotifyURI,
                                          AlbumExternalURL = t.Album.ExternalURL,
                                          Length = t.Length,
                                          DateSaved = i.DateAdded,
                                          Artists = t.Artists.Select(a =>
                                          {
                                              var artistIDs = t.TrackArtists.OrderBy(ta => ta.Index).Select(ta => ta.ArtistID).ToList();
                                              return new ArtistViewModel
                                              {
                                                  ID = a.ID,
                                                  Name = a.Name,
                                                  ExternalURL = a.ExternalURL,
                                                  Index = artistIDs.IndexOf(a.ID)
                                              };
                                          }).ToList(),
                                          DateLastPlayed = lastPlayed.FirstOrDefault(lp => lp.AliasID == i.AliasID)?.LastPlayed,
                                          IsInLikedSongs = liked != null
                                      };


                batch.TrackViewModels = trackViewModels.ToList();
                foreach (var trackViewModel in batch.TrackViewModels)
                    trackViewModel.Artists = trackViewModel.Artists.OrderBy(ta => ta.Index).ToList();
                batches.Add(batch);
            }

            return (totalItems, batches);
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
                        if(await _dataContext.TrackRecords.CountAsync(r => r.UserId == user.SpotifyUserID && r.SpotifyID == item.track.id && r.PlaylistID == null) > 0)
                        {
                            await _dataContext.SaveChangesAsync(cancellationToken);
                            return;
                        }
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
                    viewModel.Artists.Add(new Classes.ArtistViewModel
                    {
                        ID = artist.id,
                        Name = artist.name,
                        ExternalURL = artist.external_urls.spotify,
                        Index = track.artists.IndexOf(artist)
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

        public async Task<(int, List<TrackViewModel>)> SearchForTracks(Guid userID, SearchDTO searchDTO)
        {
            var tracks = new List<TrackViewModel>();
            var query = new StringBuilder();

            if(!string.IsNullOrWhiteSpace(searchDTO.Artist)) query.Append($"artist:{searchDTO.Artist} ");
            if(!string.IsNullOrWhiteSpace(searchDTO.Album)) query.Append($"album:{searchDTO.Album} ");
            if(!string.IsNullOrWhiteSpace(searchDTO.Track)) query.Append($"track:{searchDTO.Track} ");
            if(!string.IsNullOrWhiteSpace(searchDTO.Year)) query.Append($"year:{searchDTO.Year} ");
            if(!string.IsNullOrWhiteSpace(searchDTO.ISRC)) query.Append($"isrc:{searchDTO.ISRC} ");
            if(!string.IsNullOrWhiteSpace(searchDTO.Genre)) query.Append($"genre:{searchDTO.Genre} ");
            query.Remove(query.Length - 1, 1);

            var response = await _spotifyAPIWrapper.GetAsync<SearchQueryResult>(userID, $"search?q={query}&type=track&limit=10&offset={searchDTO.Offset}");

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
                tracks.Add(trackViewModel);
            }
            return (response.Tracks.total, tracks);
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
                    ReleaseDatePrecision = album.release_date_precision,
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
                albumEntity.ReleaseDatePrecision = album.release_date_precision;
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
                    AlbumSortName = albumEntity.SortName,
                    Artists = trackArtists,
                    ArtistSortName = trackArtists.First().SortName,
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
                trackEntity = _dataContext.Tracks
                    .Include(t => t.Artists)
                    .Include(t => t.TrackArtists)
                    .First(t => t.ID == track.id);
                if (trackEntity.Name != track.name)
                {
                    trackEntity.Name = track.name;
                    trackEntity.SortName = RegexHelpers.GenerateSortName(track.name);
                }
                trackEntity.ArtistSortName = trackArtists.First().SortName;
                trackEntity.AlbumID = track.album.id;
                trackEntity.Album = albumEntity;
                trackEntity.AlbumSortName = albumEntity.SortName;
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

                    var trackArtist = trackEntity.TrackArtists.FirstOrDefault(ta => ta.ArtistID == artist.ID);
                    if (trackArtist == null)
                    {
                        trackEntity.TrackArtists.Add(new TrackArtist
                        {
                            ArtistID = artist.ID,
                            TrackID = trackEntity.ID,
                            Index = trackArtists.IndexOf(artist)
                        });
                    }
                    else
                    {
                        trackArtist.Index = trackArtists.IndexOf(artist);
                    }
                }

                var artistsToRemove = trackEntity.Artists.Where(a => !trackArtists.Any(ta => ta.ID == a.ID)).ToList();
                foreach (var r in artistsToRemove) trackEntity.Artists.Remove(r);
            }
            trackEntity.GenerateMatchKey();
            return trackEntity;
        }

        /// <summary>
        /// Function to update a track on Spotify with the latest information pulled via an API call.
        /// </summary>
        /// <param name="trackID">The ID of the track.</param>
        public async Task SyncTrack(string trackID)
        {
            // Gets the first user from the database. This assumes the application is a single user application.
            // If the application was expanded to include multiple users, this would likely pull an administrator account.
            var user = await _dataContext.Users.FirstOrDefaultAsync();
            // Makes an API call with the track ID.
            var track = await _spotifyAPIWrapper.GetAsync<Classes.APIData.Track>(user.ID, $"tracks/{trackID}");
            // If track exists, updates the track information in the database.
            if (track != null)
            {
                await AddOrUpdateTrack(track);
                await _dataContext.SaveChangesAsync();
            }
        }
    }
}


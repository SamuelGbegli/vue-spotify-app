using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using vue_spotify_app.Classes;
using vue_spotify_app.Classes.APIData;
using vue_spotify_app.Classes.SortNameHelpers;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server
{
    public class PlaybackRecordService
    {
        private readonly DataContext _dataContext;
        private readonly SpotifyAPIWrapper _spotifyAPIWrapper;
        private readonly ILogger<PlaybackRecordService> _logger;
        private readonly TrackService _trackService;

        public PlaybackRecordService(DataContext dataContext, SpotifyAPIWrapper spotifyAPIWrapper,ILogger<PlaybackRecordService> logger, TrackService trackService)
        {
            _dataContext = dataContext;
            _spotifyAPIWrapper = spotifyAPIWrapper;
            _logger = logger;
            _trackService = trackService;
        }
        /// <summary>
        /// Refreshes the playback history table with data from Spotify's Recently Played list.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task UpdatePlaybackHistory(string authToken)
        {
            // Stores the initial call to the API
            var apiCall = "https://api.spotify.com/v1/me/player/recently-played?limit=50&";
            // Looks for the most recent record in the database
            var mostRecentRecord = await _dataContext.PlaybackRecords.OrderByDescending(r => r.DatePlayed).FirstOrDefaultAsync();
            // If no record is found, set call to look for all tracks played before now
            if (mostRecentRecord == null)
            {
                apiCall += $"before={((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds()}";
            }
            // Otherwise, set call to get all tracks from after the most recent record
            else
            {
                apiCall += $"after={((DateTimeOffset)mostRecentRecord.DatePlayed).ToUnixTimeMilliseconds()}";
            }

            // Sets up HTTP Client
            HttpClient client = new HttpClient();
            // Adds JSON header to client
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            // Adds Spotify auth token to header
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            // List of new records
            var newRecords = new List<PlaybackRecord>();
            // Loops until no more records can be found
            while (apiCall != null)
            {
                // Makes call to API
                HttpResponseMessage responseMessage = await client.GetAsync(apiCall);
                if (responseMessage.IsSuccessStatusCode)
                {
                    using var contentStream = responseMessage.Content.ReadAsStream();
                    // Deserialize JSON response to object
                    var playbackHistoryPage = System.Text.Json.JsonSerializer.Deserialize<PlaybackHistoryPage>(contentStream);
                    // Loops through items returned in page
                    foreach (var item in playbackHistoryPage.items)
                    {
                        // Adds a new internal record to list
                        await _dataContext.PlaybackRecords.AddAsync(new PlaybackRecord
                        {
                            DatePlayed = DateTime.Parse(item.played_at),
                            SpotifyID = item.track.id
                        });
                    }

                    // Sets API string to new page
                    apiCall = playbackHistoryPage.next;
                    Thread.Sleep(100);

                }
                else throw new Exception("Cannot connect to API. Token is likely expired.");

            }
            await _dataContext.SaveChangesAsync();

        }

        public async Task UpdatePlaybackHistory(Guid userID, CancellationToken cancellationToken = default)
        {
            var user = await _dataContext.Users.Include(u => u.SpotifyToken).FirstOrDefaultAsync(u => u.ID == userID, cancellationToken);
            if (user == null || user.SpotifyToken == null)
            {
                return;
            }
            try
            {
                // Stores the initial call to the API
                var apiCall = "me/player/recently-played?limit=50&";
                // Looks for the most recent record in the database
                var mostRecentRecord = await _dataContext.PlaybackRecords.OrderByDescending(r => r.DatePlayed).FirstOrDefaultAsync(cancellationToken);
                // If no record is found, set call to look for all tracks played before now
                if (mostRecentRecord == null)
                {
                    apiCall += $"before={((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds()}";
                }
                // Otherwise, set call to get all dtracks from after the most recent record
                else
                {
                    apiCall += $"after={((DateTimeOffset)mostRecentRecord.DatePlayed).ToUnixTimeMilliseconds()}";
                }

                const int batchSize = 200;
                var buffer = new List<PlaybackRecord>(200);

                while (apiCall != null && !cancellationToken.IsCancellationRequested)
                {
                    var playbackHistoryPage = await _spotifyAPIWrapper.GetAsync<PlaybackHistoryPage>(user.ID, apiCall);
                    foreach (var item in playbackHistoryPage.items)
                    {
                        buffer.Add(new PlaybackRecord
                        {
                            DatePlayed = DateTime.Parse(item.played_at),
                            SpotifyID = item.track.id,
                            Context = item.context?.type,
                            ContextURI = item.context?.uri
                        });
                        if(await _dataContext.Tracks.FindAsync(item.track.id) == null)
                        {
                            var trackToAdd = await _trackService.AddOrUpdateTrack(item.track);
                            await _dataContext.AddAsync(trackToAdd);
                        }
                        if (buffer.Count >= batchSize)
                        {
                            await _dataContext.PlaybackRecords.AddRangeAsync(buffer, cancellationToken);
                            await _dataContext.SaveChangesAsync(cancellationToken);
                            buffer.Clear();
                        }
                       
                    }

                    // Sets API string to new page
                    if (!string.IsNullOrWhiteSpace(playbackHistoryPage.next))
                    {
                        apiCall = playbackHistoryPage.next.Replace("https://api.spotify.com/v1/", "");
                    }
                    else apiCall = null;

                    await Task.Delay(100, cancellationToken);
                }
                if (buffer.Count > 0)
                {
                    await _dataContext.PlaybackRecords.AddRangeAsync(buffer, cancellationToken);
                    buffer.Clear();
                }
                await _dataContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating playback history for user {user.ID}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a set of playback records.
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="offset"></param>
        /// <param name="numberOfRecords"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<(int, List<PlaybackRecordViewModel>)> GetPlaybackRecords(User user, int offset = 0, int numberOfRecords = 50, DateTime? startDate = null, DateTime? endDate = null)
        {
            //TODO: add more sorting methods
            var query = _dataContext.PlaybackRecords.AsQueryable();

            // Section for if a start date and end date is provided: only gets records that are between the
            // lowest and highest values inclusive
            if (startDate != null && endDate != null)
            {
                // Creates list of date values to fetch the min and max values
                var dates = new List<DateTime>
                {
                    startDate.Value, endDate.Value
                };
                query = query.Where(r => r.DatePlayed >= dates.Min() && r.DatePlayed < dates.Max().AddDays(1));

            }
            else
            {
                if (startDate != null)
                    query = query.Where(r => r.DatePlayed >= startDate.Value);
                if (endDate != null)
                    query = query.Where(r => r.DatePlayed < endDate.Value.AddDays(1));
            }

            var totalRecords = await query.CountAsync();

            var records = await query
                .OrderByDescending(r => r.DatePlayed)
                .Skip((offset - 1) * numberOfRecords)
                .Take(numberOfRecords)
                .ToListAsync();

            var viewModels = new List<PlaybackRecordViewModel>();



            // Gets track information from Spotify
            // Makes call to API
            foreach (var record in records)
            {
                // Deserialize JSON response to object
                var track = await _dataContext.Tracks
                .Include(t => t.Artists)
                .Include(t => t.Album)
                .ThenInclude(a => a.AlbumCover)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.ID == record.SpotifyID);

                var viewModel = new PlaybackRecordViewModel
                {
                    DatePlayed = record.DatePlayed,
                    Name = track.Name,
                    TrackURL = track.ExternalURL,
                    Artists = track.Artists.Select(a => new Classes.Artist
                    {
                        Name = a.Name,
                        ExternalURL = a.ExternalURL
                    }).ToList(),
                    AlbumName = track.Album.Name,
                    AlbumLink = track.Album.ExternalURL,
                    AlbumCover = track.Album.AlbumCover.Link,
                    SpotifyID = track.ID
                };
                
                viewModel.IsInLikedSongs = _dataContext.TrackRecords.Count(t => t.SpotifyID == record.SpotifyID && t.PlaylistID == null && t.UserId == user.SpotifyUserID) > 0;
                viewModels.Add(viewModel);
            }


            return (totalRecords, viewModels);
        }

        /// <summary>
        /// Adds a new set of pending records to the main database.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task PushPendingPlaybackRecords(List<string> ids)
        {
            foreach (var id in ids)
            {
                var pendingRecord = await _dataContext.PendingPlaybackRecords.FindAsync(id);

                if (pendingRecord != null)
                {
                    var record = new PlaybackRecord
                    {
                        DatePlayed = pendingRecord.DateRecorded,
                        SpotifyID = pendingRecord.InputtedSpotifyID
                    };
                    await _dataContext.PlaybackRecords.AddAsync(record);
                    _dataContext.PendingPlaybackRecords.Remove(pendingRecord);
                }

            }
            await _dataContext.SaveChangesAsync();
        }

        public async Task<int> GetNumberOfTrackRecords(string id)
        {
            var track = await _dataContext.Tracks.FindAsync(id);
            // Block if track is found in tracks database
            if (track != null)
            {
                // Gets Spotify IDs of tracks with matching alias IDs
                var trackIDs = await _dataContext.Tracks.Where(t => t.AliasID == track.AliasID && !string.IsNullOrWhiteSpace(t.Name)).Select(t => t.ID).ToListAsync();
                // Gets all records with matching aliaa Spotify IDs
                return await _dataContext.PlaybackRecords.CountAsync(x => trackIDs.Contains(x.SpotifyID));

            }

            return await _dataContext.PlaybackRecords.CountAsync(p => p.SpotifyID == id);
        }

        /// <summary>
        /// Returns the number of times a track has been recorded as played, taking into account aliased tracks.
        /// </summary>
        /// <param name="id">The ID of the track in Spotify.</param>
        /// <param name="offset">The number of records to be skipped.</param>
        /// <param name="numberOfRecords">The number of records to be returned</param>
        /// <returns></returns>
        public async Task<List<DateTime>> GetPlaybackRecordsPerTrack(string id, int offset, int numberOfRecords)
        {
            List<PlaybackRecord> records = new List<PlaybackRecord>(); 
            // Looks for track by ID
            var track = await _dataContext.Tracks.FindAsync(id);
            // Block if track is found in tracks database
            if(track != null)
            {
                // Gets Spotify IDs of tracks with matching alias IDs
                var trackIDs = await _dataContext.Tracks.Where(t => t.AliasID == track.AliasID && !string.IsNullOrWhiteSpace(t.Name)).Select(t => t.ID).ToListAsync();
                // Gets all records with matching aliaa Spotify IDs
                records = await _dataContext.PlaybackRecords.Where(x => trackIDs.Contains(x.SpotifyID)).OrderByDescending(x => x.DatePlayed).Skip((offset - 1) * numberOfRecords).Take(numberOfRecords).ToListAsync();

            }
            // Block if track is not found in tracks database
            else
            {
                // Gets all records with matching Spotify IDs
                records = await _dataContext.PlaybackRecords.Where(x => x.SpotifyID == id).OrderByDescending(x => x.DatePlayed).Skip((offset - 1) * numberOfRecords).Take(numberOfRecords).ToListAsync();
            }
             return records.Select(x => x.DatePlayed).ToList();
        }

        /// <summary>
        /// Returns the number of pages of playback records the user can browse.
        /// </summary>
        /// <param name="recordsPerPage"></param>
        /// <returns></returns>
        public async Task<int> GetPagesOfRecords(int recordsPerPage = 50, DateTime? startDate = null, DateTime? endDate = null)
        {
            var values = 0;
            if (startDate != null && endDate != null)
            {
                var dates = new List<DateTime>
                {
                    startDate.Value, endDate.Value
                };
                values = await _dataContext.PlaybackRecords
                    .Where(r => r.DatePlayed >= dates.Min() && r.DatePlayed <= dates.Max().AddDays(1))
                    .CountAsync();
            }
            else if (startDate == null && endDate != null)
            {
                values = await _dataContext.PlaybackRecords
                    .Where(r => r.DatePlayed <= endDate.Value)
                    .CountAsync();
            }
            else if (endDate == null && startDate != null)
            {
                values = await _dataContext.PlaybackRecords
                    .Where(r => r.DatePlayed >= startDate.Value)
                    .CountAsync();
            }
            else
            {
                values = await _dataContext.PlaybackRecords.CountAsync();
            }
            return (int)Math.Ceiling(values / (decimal)recordsPerPage);
        }

        public int GetPagesOfGroupedRecords(int numberOfRecords, DateTime? startDate = null, DateTime? endDate = null)
        {
            var records = new List<IGrouping<string, PlaybackRecord>>();
            if (startDate != null && endDate != null)
            {
                var dates = new List<DateTime>
                {
                    startDate.Value, endDate.Value
                };
                records = _dataContext.PlaybackRecords
                    .Where(r => r.DatePlayed >= dates.Min() && r.DatePlayed <= dates.Max().AddDays(1))
                    .AsEnumerable()
                    .GroupBy(r => r.SpotifyID)
                    .ToList();
            }
            else if (startDate == null && endDate != null)
            {
                records = _dataContext.PlaybackRecords
                    .Where(r => r.DatePlayed <= endDate.Value)
                    .AsEnumerable()
                    .GroupBy(r => r.SpotifyID)
                    .ToList();
            }
            else if (endDate == null && startDate != null)
            {
                records = _dataContext.PlaybackRecords
                    .Where(r => r.DatePlayed >= startDate.Value)
                    .AsEnumerable()
                    .GroupBy(r => r.SpotifyID)
                    .ToList();
            }
            else
            {
                records = _dataContext.PlaybackRecords
                    .AsEnumerable()
                    .GroupBy(r => r.SpotifyID)
                    .ToList();
            }
            return (int)Math.Ceiling(records.Count() / (double)numberOfRecords);
        }

        /// <summary>
        /// Returns tracks and the number of matching playback records.
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="offset"></param>
        /// <param name="numberOfRecords"></param>
        /// <returns></returns>
        public async Task<List<TrackViewModel>> GetTracksGroupedByPlaybackRecords(Guid userId, int offset = 0, int numberOfRecords = 50, DateTime? startDate = null, DateTime? endDate = null)
        {
            var viewModels = new List<TrackViewModel>();
            var records = new List<IGrouping<string, PlaybackRecord>>();
            if (startDate != null && endDate != null)
            {
                var dates = new List<DateTime>
                {
                    startDate.Value, endDate.Value
                };
                records = _dataContext.PlaybackRecords
                    .Where(r => r.DatePlayed >= dates.Min() && r.DatePlayed <= dates.Max().AddDays(1))
                    .AsEnumerable()
                    .GroupBy(r => r.SpotifyID)
                    .OrderByDescending(g => g.Count())
                    .Skip((offset - 1) * numberOfRecords)
                    .Take(numberOfRecords)
                    .ToList();
            }
            else if (startDate == null && endDate != null)
            {
                records = _dataContext.PlaybackRecords
                    .Where(r => r.DatePlayed <= endDate.Value)
                    .AsEnumerable()
                    .GroupBy(r => r.SpotifyID)
                    .OrderByDescending(g => g.Count())
                    .Skip((offset - 1) * numberOfRecords)
                    .Take(numberOfRecords)
                    .ToList();
            }
            else if (endDate == null && startDate != null)
            {
                records = _dataContext.PlaybackRecords
                    .Where(r => r.DatePlayed >= startDate.Value)
                    .AsEnumerable()
                    .GroupBy(r => r.SpotifyID)
                    .OrderByDescending(g => g.Count())
                    .Skip((offset - 1) * numberOfRecords)
                    .Take(numberOfRecords)
                    .ToList();
            }
            else
            {
                records = _dataContext.PlaybackRecords
                    .AsEnumerable()
                    .GroupBy(r => r.SpotifyID)
                    .OrderByDescending(g => g.Count())
                    .Skip((offset - 1) * numberOfRecords)
                    .Take(numberOfRecords)
                    .ToList();
            }

            var trackRecords = records.ToList();


            foreach (var trackRecord in trackRecords)
            {

                var track = await _spotifyAPIWrapper.GetAsync<Classes.APIData.Track>(userId, $"tracks/{trackRecord.Key}");

                var viewModel = new TrackViewModel
                {
                    ID = trackRecord.Key,
                    Name = track.name,
                    ExternalURL = track.external_urls.spotify,
                    AlbumName = track.album.name,
                    AlbumExternalURL = track.album.external_urls.spotify,
                    AlbumCover = track.album.images.LastOrDefault().url,
                    NumberOfFoundRecords = trackRecord.Count()
                };

                foreach (var artist in track.artists)
                {
                    viewModel.Artists.Add(new Classes.Artist
                    {
                        Name = artist.name,
                        ExternalURL = artist.external_urls.spotify
                    });
                }


                viewModels.Add(viewModel);

            }
            return viewModels;
        }

        public async Task<(int, List<TrackViewModel>)> GetTracksGroupedByPlaybackRecordsNew(Guid userId, int offset = 0, int numberOfRecords = 50, DateTime? startDate = null, DateTime? endDate = null)
        {

            var viewModels = new List<TrackViewModel>();

            List<DateTime> dates = new List<DateTime>();

            if (startDate != null && endDate != null)
            {
                dates = new List<DateTime>
                {
                    startDate.Value, endDate.Value
                };
            }
            var query =
            from r in _dataContext.PlaybackRecords
            join t in _dataContext.Tracks on r.SpotifyID equals t.ID
            where (startDate == null || (dates.IsNullOrEmpty() && r.DatePlayed >= startDate) || (!dates.IsNullOrEmpty() &&  r.DatePlayed >= dates.Min()))
            && (endDate == null || (dates.IsNullOrEmpty() && r.DatePlayed < endDate.Value.AddDays(1)) || (!dates.IsNullOrEmpty() && r.DatePlayed <= dates.Max().AddDays(1)))
            group r by t.AliasID into g
            select new
            {
                ID = g.Select(x => x.SpotifyID).FirstOrDefault(),
                Count = g.Count()
            };

            var totalRecords = await query.CountAsync();

            var trackRecords = await query
                .OrderByDescending(r => r.Count)
                .Skip((offset - 1) * numberOfRecords)
                .Take(numberOfRecords).ToListAsync();

            foreach (var trackRecord in trackRecords)
            {

                var track = await _spotifyAPIWrapper.GetAsync<Classes.APIData.Track>(userId, $"tracks/{trackRecord.ID}");

                var viewModel = new TrackViewModel
                {
                    ID = trackRecord.ID,
                    Name = track.name,
                    ExternalURL = track.external_urls.spotify,
                    AlbumName = track.album.name,
                    AlbumExternalURL = track.album.external_urls.spotify,
                    AlbumCover = track.album.images.LastOrDefault().url,
                    NumberOfFoundRecords = trackRecord.Count
                };

                foreach (var artist in track.artists)
                {
                    viewModel.Artists.Add(new Classes.Artist
                    {
                        Name = artist.name,
                        ExternalURL = artist.external_urls.spotify
                    });
                }


                viewModels.Add(viewModel);

            }
            return (totalRecords, viewModels);
        }

        public async Task SyncTracksByPlaybackHistory(Guid userId, CancellationToken cancellationToken = default)
        {
            const int bufferSize = 50;
            List<Classes.Track> trackBuffer = new List<Classes.Track>(bufferSize);
            foreach (var record in await _dataContext.PlaybackRecords.ToListAsync())
            {
                if (!await _dataContext.Tracks.AnyAsync(t => t.ID == record.SpotifyID, cancellationToken) && !trackBuffer.Any(t => t.ID == record.SpotifyID))
                {
                    var item = await _spotifyAPIWrapper.GetAsync<Classes.APIData.Track>(userId, $"tracks/{record.SpotifyID}");

                    var track = await _trackService.AddOrUpdateTrack(item);
                    trackBuffer.Add(track);
                }
                if (trackBuffer.Count >= bufferSize)
                {
                    
                    foreach (var track in trackBuffer)
                    {
                        Debug.WriteLine($"Information about track with ID {track.ID} - Track: {track.Name}, Album: {track.Album.Name}, Artists: {string.Join(", ", track.Artists.Select(a => a.Name))}");
                    }
                    
                    await _dataContext.Tracks.AddRangeAsync(trackBuffer);
                    await _dataContext.SaveChangesAsync();
                    Debug.WriteLine("Saved batch of tracks to database.");
                    trackBuffer.Clear();
                }
            }
                if (trackBuffer.Count > 0)
                {
                foreach (var track in trackBuffer)
                {
                    Debug.WriteLine($"Information about track with ID {track.ID} - Track: {track.Name}, Album: {track.Album.Name}, Artists: {string.Join(", ", track.Artists.Select(a => a.Name))}");
                }

                await _dataContext.Tracks.AddRangeAsync(trackBuffer);
                await _dataContext.SaveChangesAsync();
                Debug.WriteLine("Saved batch of tracks to database.");
                trackBuffer.Clear();
            }
        }
   
        
    }
}

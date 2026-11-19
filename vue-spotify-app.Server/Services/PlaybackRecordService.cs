using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2019.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
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

        public PlaybackRecordService(DataContext dataContext, SpotifyAPIWrapper spotifyAPIWrapper, ILogger<PlaybackRecordService> logger, TrackService trackService)
        {
            _dataContext = dataContext;
            _spotifyAPIWrapper = spotifyAPIWrapper;
            _logger = logger;
            _trackService = trackService;
        }
        /// <summary>
        /// Refreshes the playback history table with data from Spotify's Recently Played list.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
                // Otherwise, set call to get all tracks from after the most recent record
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
                            UserID = user.ID,
                            SpotifyID = item.track.id,
                            Context = item.context?.type,
                            ContextURI = item.context?.uri
                        });
                        var track = await _dataContext.Tracks.FindAsync(item.track.id);
                        if (track == null)
                        {
                            track = await _trackService.AddOrUpdateTrack(item.track);
                            await _dataContext.AddAsync(track);
                        }
                        else
                        {
                            var alias = await _dataContext.TrackAliases
                                .Include(ta => ta.Statistics)
                                .Include(ta => ta.Tracks)
                                .FirstOrDefaultAsync(ta => ta.ID == track.AliasID);
                            if (alias != null)
                            {
                                if (alias.Statistics == null)
                                    alias.Statistics = new TrackAliasPlaybackStatistics
                                    {
                                        UserID = user.ID,
                                        TrackAliasID = alias.ID
                                    };
                                alias.Statistics.LastUpdated = DateTime.Parse(item.played_at);
                                alias.Statistics.TotalRecordedPlays = await _dataContext.PlaybackRecords
                                    .Where(pr => pr.UserID == user.ID && alias.Tracks.Select(t => t.ID).Contains(pr.SpotifyID)).CountAsync();
                                alias.Statistics.LastUpdated = DateTime.Now;
                            }
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
            var query = _dataContext.PlaybackRecords.Where(r => r.UserID == user.ID);

            // Section for if a start date and end date is provided: only gets records that are between the
            // lowest and highest values inclusive
            if (startDate != null)
                query = query.Where(r => r.DatePlayed >= startDate.Value);
            if (endDate != null)
                query = query.Where(r => r.DatePlayed < endDate.Value.AddDays(1));

            var totalRecords = await query.CountAsync();

            var records = await query
                .OrderByDescending(r => r.DatePlayed)
                .Skip((offset - 1) * numberOfRecords)
                .Take(numberOfRecords)
                .Select(r => new
                {
                    r.SpotifyID,
                    r.DatePlayed
                })
                .ToListAsync();

            var trackIDs = records
                .Select(r => r.SpotifyID)
                .Distinct()
                .ToList();

            var tracks = await _dataContext.Tracks
                .AsNoTracking()
                .Include(t => t.TrackArtists)
                .ThenInclude(ta => ta.Artist)
                .Where(t => trackIDs.Contains(t.ID))
                .Select(t => new
                {
                    t.ID,
                    t.Name,
                    t.ExternalURL,

                    Artists = t.TrackArtists,

                    AlbumName = t.Album.Name,
                    AlbumCover = t.Album.AlbumCover.Link,
                    AlbumLink = t.Album.ExternalURL,

                })
                .ToListAsync();

            var tracksByID = tracks.ToDictionary(t => t.ID);

            var viewModels = records
                .Where(r => tracksByID.ContainsKey(r.SpotifyID))
                .Select(r =>
               {
                   var track = tracksByID[r.SpotifyID];
                   return new PlaybackRecordViewModel
                   {
                       SpotifyID = track.ID,
                       Name = track.Name,
                       TrackURL = track.ExternalURL,
                       DatePlayed = r.DatePlayed,
                       AlbumName = track.AlbumName,
                       AlbumCover = track.AlbumCover,
                       AlbumLink = track.AlbumLink,
                       Artists = track.Artists.Select(ta => new ArtistViewModel
                       {
                           Name = ta.Artist.Name,
                           ExternalURL = ta.Artist.ExternalURL
                       }).ToList()
                   };
               }).ToList();

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

        //TODO: remove
        public async Task<int> GetNumberOfTrackRecords(string id)
        {
            var track = await _dataContext.Tracks.FindAsync(id);
            // Block if track is found in tracks database
            if (track != null)
            {
                // Gets Spotify IDs of tracks with matching alias IDs
                var trackIDs = await _dataContext.Tracks.Where(t => t.AliasID == track.AliasID && !string.IsNullOrWhiteSpace(t.Name)).Select(t => t.ID).ToListAsync();
                // Gets all records with matching alias Spotify IDs
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
            if (track != null)
            {
                // Gets Spotify IDs of tracks with matching alias IDs
                var trackIDs = await _dataContext.Tracks.Where(t => t.AliasID != null && t.AliasID == track.AliasID && !string.IsNullOrWhiteSpace(t.Name)).Select(t => t.ID).ToListAsync();
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

        public async Task<(int, List<TrackViewModel>)> GetTracksGroupedByPlaybackRecords(
    Guid userId,
    int offset = 0,
    int numberOfRecords = 50,
    DateTime? startDate = null,
    DateTime? endDate = null
    )
        {
            TimeSpan countTime, recordTime, trackTime, castTime;

            var query = _dataContext.PlaybackRecords
                .AsNoTracking()
                .Join(_dataContext.Tracks, pr => pr.SpotifyID, t => t.ID, (pr, t) => new
                {
                    pr.UserID,
                    t.AliasID,
                    t.Length,
                    pr.DatePlayed
                })
                .Where(r => r.UserID == userId);

            // Section for if a start date and end date is provided: only gets records that are between the
            // lowest and highest values inclusive
            if (startDate != null)
                query = query.Where(r => r.DatePlayed >= startDate.Value);
            if (endDate != null)
                query = query.Where(r => r.DatePlayed < endDate.Value.AddDays(1));

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var totalRecords = await query.GroupBy(r => r.AliasID).CountAsync();
            stopwatch.Stop();
            countTime = stopwatch.Elapsed;
            stopwatch.Restart();

            var records = await query
                .GroupBy(r => r.AliasID)
                .OrderByDescending(r => r.Count())
                .ThenByDescending(r => r.First().Length)
                .Skip((offset - 1) * numberOfRecords)
                .Take(numberOfRecords)
                .Select(r => new
                {
                    Alias = r.Key,
                    Total = r.Count(),
                    Length = r.First().Length
                }).ToListAsync();


            stopwatch.Stop();
            recordTime = stopwatch.Elapsed;

            var aliasIDs = records
                .Select(r => r.Alias)
                .Distinct()
                .ToList();

            stopwatch.Restart();

            var tracks = await _dataContext.TrackAliases
                .AsNoTracking()
                .Join(_dataContext.Tracks,
                    ta => ta.PrimaryTrackID,
                    t => t.ID,
                    (ta, t) => t
                    )
                .Where(t => aliasIDs.Contains(t.AliasID))
                .Select(t => new
                {
                    t.ID,
                    t.Name,
                    t.ExternalURL,
                    t.AliasID,
                    t.Length,

                    AlbumName = t.Album.Name,
                    AlbumCover = t.Album.AlbumCover.Link,
                    AlbumLink = t.Album.ExternalURL

                })
                .ToListAsync();


            stopwatch.Stop();
            trackTime = stopwatch.Elapsed;

            var tracksByID = tracks.ToDictionary(t => t.AliasID);
            var trackIDs = tracks.Select(t => t.ID).Distinct();

            var trackArtists = await _dataContext.TrackArtists
                .AsNoTracking()
                .Include(ta => ta.Artist)
                .Where(ta => trackIDs.Contains(ta.TrackID))
                .GroupBy(ta => ta.TrackID)
                .ToDictionaryAsync(ta => ta.Key, ta => ta.ToList());



            stopwatch.Restart();

            var viewModels = records
                .Where(r => tracksByID.ContainsKey(r.Alias))
                .Select(r =>
                {
                    var track = tracksByID[r.Alias];
                    var artists = trackArtists[track.ID];

                    return new TrackViewModel
                    {
                        ID = track.ID,
                        Name = track.Name,
                        ExternalURL = track.ExternalURL,
                        AlbumName = track.AlbumName,
                        AlbumCover = track.AlbumCover,
                        AlbumExternalURL = track.AlbumLink,
                        Artists = artists.OrderBy(a => a.Index).Select(a => new ArtistViewModel
                        {
                            Name = a.Artist.Name,
                            ExternalURL = a.Artist.ExternalURL
                        }).ToList(),
                        Length = track.Length,
                        NumberOfFoundRecords = r.Total
                    };
                }).ToList();

            stopwatch.Stop();
            castTime = stopwatch.Elapsed;

            return (totalRecords, viewModels);
        }

        public async Task<byte[]> GeneratePlaybackReport(User user, DateTime? startDate, DateTime? endDate)
        {
            var workbook = new XLWorkbook();

            var userLikedSongsList = await _dataContext.TrackLists.FirstAsync(l => l.UserID == user.SpotifyUserID && l.TrackListType == TrackListType.LikedSongs);

            var likedSongsIDs = (await _dataContext.TrackRecords
                .Join(_dataContext.Tracks, tr => tr.SpotifyID, t => t.ID, (tr, t) => new
                {
                    tr.SpotifyID,
                    tr.TrackListID,
                    t.AliasID,
                    tr.DateAdded,
                    tr.UserId
                })
                .Where(tr => tr.TrackListID == userLikedSongsList.ID && tr.UserId == user.SpotifyUserID)
                .Distinct()
                .Select(tr => new
                {
                    tr.AliasID,
                    tr.SpotifyID,
                    tr.DateAdded
                })
                .ToListAsync())
                .ToHashSet();

            var top100Sheet = workbook.Worksheets.Add("Top 100 Tracks");
            string[] headers =
            {
                "Rank",
                "ID",
                "Track",
                "Artist(s)",
                "Album",
                "Recorded plays",
                "Length",
                "Added to Liked Songs (includes equivalents)"
            };

            WriteHeaders(top100Sheet, headers);

            top100Sheet.Style.Alignment.WrapText = true;

            top100Sheet.Column("A").Width = 10;
            top100Sheet.Column("B").Width = 32;
            top100Sheet.Columns("C:E").Width = 40;
            top100Sheet.Column("F").Width = 18;
            top100Sheet.Column("G").Width = 10;
            top100Sheet.Column("H").Width = 18;
            top100Sheet.Column(7).Style.NumberFormat.Format = "m:ss";
            top100Sheet.Column(8).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";

            var top100Records = await GetTracksGroupedByPlaybackRecords(
                user.ID,
                1,
                100,
                startDate,
                endDate
                );

            var trackEntities = await _dataContext.Tracks
                .Where(t => top100Records.Item2.Select(r => r.ID).Contains(t.ID))
                .ToListAsync();

            for (int i = 2; i < 102; i++)
            {
                var track = top100Records.Item2[i - 2];
                var trackAliasID = trackEntities.Find(t => t.ID == track.ID).AliasID;
                var trackRecord = likedSongsIDs.FirstOrDefault(r => r.AliasID == trackAliasID);

                top100Sheet.Cell(i, 1).Value = i - 1;
                top100Sheet.Cell(i, 2).Value = track.ID;
                top100Sheet.Cell(i, 3).Value = track.Name;
                top100Sheet.Cell(i, 4).Value = string.Join(", ", track.Artists.Select(a => a.Name));
                top100Sheet.Cell(i, 5).Value = track.AlbumName;
                top100Sheet.Cell(i, 6).Value = track.NumberOfFoundRecords;
                top100Sheet.Cell(i, 7).Value = TimeSpan.FromMilliseconds(track.Length).TotalDays;
                top100Sheet.Cell(i, 8).Value = trackRecord != null ? trackRecord.DateAdded : "n/a";
            }

            FormatSheet(top100Sheet, 101, headers.Length);

            if (top100Records.Item2.Count == 100)
            {
                var hundrethTrack = top100Records.Item2.LastOrDefault();

                if (hundrethTrack != null)
                {
                    var candidateTracksSheet = workbook.Worksheets.Add("Top 100 Candidates");
                    WriteHeaders(candidateTracksSheet, headers);

                    candidateTracksSheet.Style.Alignment.WrapText = true;

                    candidateTracksSheet.Column("A").Width = 10;
                    candidateTracksSheet.Column("B").Width = 32;
                    candidateTracksSheet.Columns("C:E").Width = 40;
                    candidateTracksSheet.Column("F").Width = 18;
                    candidateTracksSheet.Column("G").Width = 10;
                    candidateTracksSheet.Column("H").Width = 18;
                    candidateTracksSheet.Column(7).Style.NumberFormat.Format = "m:ss";
                    candidateTracksSheet.Column(8).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";

                    var minimumPlayCount = Math.Clamp(hundrethTrack.NumberOfFoundRecords - 1, 1, hundrethTrack.NumberOfFoundRecords - 1);
                    var row = 2;
                    var page = 2;

                    var endReached = false;

                    while (!endReached)
                    {
                        var records = await GetTracksGroupedByPlaybackRecords(
                            user.ID,
                            page,
                            100,
                            startDate,
                            endDate
                        );
                        if (records.Item2.IsNullOrEmpty())
                        {
                            endReached = true;
                            break;
                        }
                        trackEntities = await _dataContext.Tracks
                            .Where(t => records.Item2.Select(r => r.ID).Contains(t.ID))
                            .ToListAsync();
                        foreach (var record in records.Item2)
                        {

                            if (minimumPlayCount > record.NumberOfFoundRecords)
                            {
                                endReached = true;
                                break;
                            }
                            else
                            {
                                var trackAliasID = trackEntities.Find(t => t.ID == record.ID).AliasID;
                                var trackRecord = likedSongsIDs.FirstOrDefault(r => r.AliasID == trackAliasID);

                                candidateTracksSheet.Cell(row, 1).Value = 99 + row;
                                candidateTracksSheet.Cell(row, 2).Value = record.ID;
                                candidateTracksSheet.Cell(row, 3).Value = record.Name;
                                candidateTracksSheet.Cell(row, 4).Value = string.Join(", ", record.Artists.Select(a => a.Name));
                                candidateTracksSheet.Cell(row, 5).Value = record.AlbumName;
                                candidateTracksSheet.Cell(row, 6).Value = record.NumberOfFoundRecords;
                                candidateTracksSheet.Cell(row, 7).Value = TimeSpan.FromMilliseconds(record.Length).TotalDays;
                                candidateTracksSheet.Cell(row, 8).Value = trackRecord != null ? trackRecord.DateAdded : "n/a";

                                row++;
                            }
                        }
                        page++;
                    }

                    FormatSheet(candidateTracksSheet, row - 1, headers.Length);
                }
            }

            var playbackDistibutionSheet = workbook.Worksheets.Add("Top 100 Playback Distribution");
            string[] distributionHeaders = {
                "Timestamp",
                "Track",
                "ID"
            };
            
            WriteHeaders(playbackDistibutionSheet, distributionHeaders);

            playbackDistibutionSheet.Style.Alignment.WrapText = true;

            playbackDistibutionSheet.Column("A").Width = 18;
            playbackDistibutionSheet.Column("B").Width = 40;
            playbackDistibutionSheet.Column("C").Width = 32;

            playbackDistibutionSheet.Column(1).Style.DateFormat.Format = "dd/mm/yyyy hh:mm";

            var distributionRow = 2;
            var trackAliases = await _dataContext.Tracks.Where(t => top100Records.Item2.Select(r => r.ID).Contains(t.ID)).Select(t => t.AliasID).ToListAsync();
            var trackIDs = await _dataContext.Tracks.Where(t => t.AliasID != null && trackAliases.Contains(t.AliasID) && !string.IsNullOrWhiteSpace(t.Name)).Select(t => t.ID).ToListAsync();
            var filteredRecords = _dataContext.PlaybackRecords
                .Where(x => trackIDs.Contains(x.SpotifyID))
                .Join(_dataContext.Tracks.Include(t => t.Alias),
                    pr => pr.SpotifyID,
                    t => t.ID,
                    (pr, t) => new
                    {
                        ID = t.Alias.PrimaryTrackID ?? t.ID,
                        pr.DatePlayed,
                        t.Name,
                    });

            if (startDate.HasValue)
                filteredRecords = filteredRecords.Where(r => r.DatePlayed >= startDate.Value);

            if (endDate.HasValue)
                filteredRecords = filteredRecords.Where(r => r.DatePlayed < endDate.Value.AddDays(1));

            var results = await filteredRecords.OrderBy(r => r.DatePlayed).ToListAsync();

            foreach (var record in results)
            {
                playbackDistibutionSheet.Cell(distributionRow, 1).Value = record.DatePlayed;
                playbackDistibutionSheet.Cell(distributionRow, 2).Value = record.Name;
                playbackDistibutionSheet.Cell(distributionRow, 3).Value = record.ID;
                distributionRow++;
            };

            FormatSheet(playbackDistibutionSheet, distributionRow - 1, distributionHeaders.Length);

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);
            return stream.ToArray();
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

        private void WriteHeaders(IXLWorksheet worksheet, string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }
            worksheet.Row(1).Style.Font.Bold = true;
        }

        private void FormatSheet(IXLWorksheet worksheet, int lastRow, int lastColumn)
        {
            if (lastRow >= 2)
            {
                var table = worksheet
                    .Range(1, 1, lastRow, lastColumn)
                    .CreateTable();

                table.Theme = XLTableTheme.TableStyleMedium1;
            }

            worksheet.SheetView.FreezeRows(1);

            /*for (int column = 1; column <= lastColumn; column++)
            {
                worksheet.Column(column).AdjustToContents();
                worksheet.Column(column).Width = Math.Min(
                    worksheet.Column(column).Width + 2,
                    60);
            }*/
        }
    }
}

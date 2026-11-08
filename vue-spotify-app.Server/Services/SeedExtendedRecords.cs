using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using vue_spotify_app.Classes;
using vue_spotify_app.Classes.APIData;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server
{
    public class SeedExtendedRecordsService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SeedExtendedRecordsService> _logger;

        public SeedExtendedRecordsService(IServiceScopeFactory scopeFactory, ILogger<SeedExtendedRecordsService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var recordsToAdd = new List<ExtendedRecord>();
                var tracksToAdd = new List<Classes.Track>();

                JsonSerializerOptions _options = new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                var trackService = scope.ServiceProvider.GetRequiredService<TrackService>();
                var spotifyAPIWrapper = scope.ServiceProvider.GetRequiredService<SpotifyAPIWrapper>();

                var user = await dataContext.Users.FirstOrDefaultAsync();

                var folderPath = Path.Combine(AppContext.BaseDirectory, "SeedData");
                var files = Directory.GetFiles(folderPath, "Streaming_History_Audio*.json");
                var logFile = Path.Combine(folderPath, "SeedLog.txt");

                using (var streamWriter = new StreamWriter(logFile))
                {
                    foreach (var file in files)
                    {
                        await using var stream = File.OpenRead(file);

                        streamWriter.WriteLine($"{DateTime.Now}\tSeeding extended records from file: {file}");

                        await foreach (var record in JsonSerializer.DeserializeAsyncEnumerable<ExtendedRecordDTO>(stream, _options))
                        {
                            if (record != null)
                            {
                                var trackID = record.spotify_track_uri.Replace("spotify:track:", "");
                                if (!tracksToAdd.Select(t => t.ID).Contains(trackID))
                                {
                                    var track = await dataContext.Tracks.FirstOrDefaultAsync(t => t.ID == trackID);
                                    if (track == null)
                                    {
                                        var apiResponse = await spotifyAPIWrapper.GetAsync<Classes.APIData.Track>(user.ID, $"tracks/{trackID}");
                                        track = await trackService.AddOrUpdateTrack(apiResponse);
                                        tracksToAdd.Add(track);
                                    }
                                }


                                recordsToAdd.Add(new ExtendedRecord
                                {
                                    TrackURI = record.spotify_track_uri,
                                    AmountPlayed = record.ms_played,
                                    Shuffle = record.shuffle,
                                    OfflineTimestamp = record.offline_timestamp,
                                    ReasonStart = record.reason_start,
                                    ReasonEnd = record.reason_end,
                                    Timestamp = DateTime.SpecifyKind(
                                        DateTime.Parse(record.ts),
                                        DateTimeKind.Utc),
                                    Platform = record.platform
                                });
                            }

                            if (recordsToAdd.Count >= 1000)
                            {
                                await dataContext.ExtendedRecords.AddRangeAsync(recordsToAdd);
                                await dataContext.SaveChangesAsync();
                                recordsToAdd.Clear();
                                streamWriter.WriteLine($"{DateTime.Now}\tSaved 1,000 records to database");
                            }

                            if (tracksToAdd.Count >= 100)
                            {
                                await dataContext.Tracks.AddRangeAsync(tracksToAdd);
                                await dataContext.SaveChangesAsync();
                                tracksToAdd.Clear();
                                streamWriter.WriteLine($"{DateTime.Now}\tSaved 100 new tracks to database");
                            }
                        }

                        await Task.Delay(500);
                        streamWriter.WriteLine($"{DateTime.Now}\tReached end of {file}");
                    }

                    if (recordsToAdd.Count > 0)
                    {
                        await dataContext.ExtendedRecords.AddRangeAsync(recordsToAdd);
                        await dataContext.SaveChangesAsync();
                        streamWriter.WriteLine($"{DateTime.Now}\tSaved {recordsToAdd.Count} records to database");
                    }
                    if (tracksToAdd.Count > 0)
                    {
                        await dataContext.Tracks.AddRangeAsync(tracksToAdd);
                        await dataContext.SaveChangesAsync();
                        streamWriter.WriteLine($"{DateTime.Now}\tSaved {tracksToAdd.Count} new tracks to database");
                    }

                    foreach (var file in files)
                    {
                        File.Delete(file);
                        streamWriter.WriteLine($"{DateTime.Now}\tDeleted {file}");
                    }
                }
            }
        }
    }
}

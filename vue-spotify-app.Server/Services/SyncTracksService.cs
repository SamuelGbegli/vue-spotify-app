using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Services
{
    public class SyncTracksService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public SyncTracksService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var trackService = scope.ServiceProvider.GetRequiredService<TrackService>();
                    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();

                    var numberOfTracks = await dataContext.Tracks.CountAsync(stoppingToken);
                    for (int i = 0; i < numberOfTracks; i += 50)
                    {
                        if (stoppingToken.IsCancellationRequested) break;
                        try
                        {
                            var tracks = await dataContext.Tracks.AsNoTracking().Skip(i).Take(50).ToListAsync(stoppingToken);
                            foreach (var track in tracks)
                            {
                                if (stoppingToken.IsCancellationRequested) break;
                                var numberOfRetrys = 0;
                                while(numberOfRetrys < 3)
                                {
                                    try
                                    {
                                        await trackService.SyncTrack(track.ID);
                                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                                        break;
                                    }
                                    catch (Exception ex)
                                    {
                                        numberOfRetrys++;
                                        await Task.Delay(TimeSpan.FromSeconds(10 * numberOfRetrys), stoppingToken);
                                    }
                                }
                            }
                            await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"ERROR: {ex.Message}");
                            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                        }
                    }
                }
            }
        }
    }
}

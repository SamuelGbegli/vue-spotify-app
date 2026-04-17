
using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Services
{
    public class SyncPlaybackRecordService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SyncPlaybackRecordService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    List<Guid> userIDs;
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                        userIDs = await dataContext.Users.AsNoTracking().Select(u => u.ID).ToListAsync(stoppingToken);

                        foreach (var id in userIDs)
                        {
                            if (stoppingToken.IsCancellationRequested) break;

                            using (var userScope = _scopeFactory.CreateScope())
                            {

                                var playbackRecordService = userScope.ServiceProvider.GetRequiredService<PlaybackRecordService>();
                                await playbackRecordService.UpdatePlaybackHistory(id, stoppingToken);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    // Log the exception (you can use any logging framework you prefer)
                    Console.WriteLine($"An error occurred while syncing playback records: {ex.Message}");
                }
                // Wait for 5 minutes before checking again
                
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}

using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Services
{
    public class SyncTracksWithPlaybackHistoryService :BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SyncTracksWithPlaybackHistoryService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
                        var userID = await dataContext.Users.Select(u => u.ID).FirstOrDefaultAsync(stoppingToken);
                        var playbackRecordService = scope.ServiceProvider.GetRequiredService<PlaybackRecordService>();
                        await playbackRecordService.SyncTracksByPlaybackHistory(userID, stoppingToken);
                    }
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Wait for 1 hour before the next sync
                }
                catch (Exception ex)
                {
                    // Log the exception (you can use any logging framework you prefer)
                    System.Diagnostics.Debug.WriteLine($"An error occurred while syncing tracks with playback history: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait for 5 minutes before retrying
                }
            }
        }
    }     
}

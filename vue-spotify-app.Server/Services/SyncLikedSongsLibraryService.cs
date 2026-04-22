using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Services
{
    public class SyncLikedSongsLibraryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SyncLikedSongsLibraryService(IServiceScopeFactory scopeFactory)
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
                                var likedSongsLibraryService = userScope.ServiceProvider.GetRequiredService<TrackService>();
                                await likedSongsLibraryService.InitialiseTracks(id, cancellationToken: stoppingToken);
                            }
                        }
                    }

                    // Wait for 120 minutes before checking again
                    await Task.Delay(TimeSpan.FromMinutes(120), stoppingToken);
                }
                catch (Exception ex)
                {
                    // Log the exception (you can use any logging framework you prefer)
                    Debug.WriteLine($"An error occurred while syncing liked songs: {ex.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            }
        }
            
    }
}

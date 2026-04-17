
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Services
{
    public class SyncPlaylistService : BackgroundService
    {

        private readonly IServiceScopeFactory _scopeFactory;

        public SyncPlaylistService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int deferredWaitTime = 5;
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
                                var playlistService = scope.ServiceProvider.GetRequiredService<PlaylistService>();
                                await playlistService.InitialisePlaylists(id);
                                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
                                await playlistService.InitialisePlaylistTracks(id);
                                await Task.Delay(TimeSpan.FromMinutes(10));
                            }
                        }
                    }
                    deferredWaitTime = 5;
                    await Task.Delay(TimeSpan.FromHours(2));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ERROR: {ex.Message}");
                    if(deferredWaitTime < 480)
                    {
                        if (deferredWaitTime < 15) deferredWaitTime += 5;
                        else deferredWaitTime += 30;
                    }
                    await Task.Delay(TimeSpan.FromMinutes(deferredWaitTime), stoppingToken);
                }
            }
        }
    }
}

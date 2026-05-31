
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Services
{
    public class SyncTrackAliasService: BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SyncTrackAliasService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;   
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using(var scope = _scopeFactory.CreateScope())
                    {
                        var dataContext = scope.ServiceProvider.GetService<DataContext>();
                        for (int i = 0; i < await dataContext.Tracks.CountAsync(); i += 100)
                        {
                            var tracks =  dataContext.Tracks.Include(t => t.Artists).Skip(i).Take(100).ToList();
                            foreach (var track in tracks)
                                track.GenerateMatchKey();
                        }
                        await dataContext.SaveChangesAsync();

                        var trackAliasService = scope.ServiceProvider.GetRequiredService<TrackAliasService>();
                        await trackAliasService.GenerateAliases();
                    }
                    await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
                }
                catch (Exception e) 
                {
                    Debug.WriteLine($"An error occured: ${e.Message}");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }
}

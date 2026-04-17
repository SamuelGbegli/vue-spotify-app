using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Classes.APIData;
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
                    // Your background task logic here
                    // TODO: Implement the logic to sync tracks in database
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}

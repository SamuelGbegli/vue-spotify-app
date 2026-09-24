using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Server.Data;
using vue_spotify_app.Server.Extensions;

namespace vue_spotify_app.Server
{
    public static class SeedTrackAliases
    {
        public static async Task Run(DataContext context)
        {
            var count = await context.TrackAliases.CountAsync();

            for (int i = 0; i < count; i+= 100)
            {
                var aliases = await context.TrackAliases.Include(a => a.Tracks).Skip(i).Take(100).ToListAsync();
                foreach(var alias in aliases)
                {
                    if(!alias.Tracks.IsNullOrEmpty())
                    {
                        var track = alias.Tracks.First();
                        alias.Name = track.Name;
                        alias.PrimaryTrackID = track.ID;
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}

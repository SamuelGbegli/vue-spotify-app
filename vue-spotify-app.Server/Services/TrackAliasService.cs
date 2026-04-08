using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using vue_spotify_app.Classes;
using vue_spotify_app.Classes.SortNameHelpers;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Services
{
    public class TrackAliasService
    {
        private readonly DataContext _dataContext;

        public TrackAliasService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task GenerateAliases()
        {
            int aliasCount = 0;
            const int trackBatchCount = 100;
            for(int i = 0; i < await _dataContext.Tracks.Include(t => t.Alias).CountAsync(); i += trackBatchCount)
            {
                var tracks = await _dataContext.Tracks.Include(t => t.Alias).Skip(i).Take(trackBatchCount).ToListAsync();

                foreach (var track in tracks)
                {
                    if (track.AliasID == null)
                    {
                        var alias = await _dataContext.TrackAliases.Include(a => a.Tracks).FirstOrDefaultAsync(a => a.Tracks.Any(t => t.ISRC == track.ISRC || (!string.IsNullOrWhiteSpace(t.MatchKey) && t.MatchKey == track.MatchKey)));
                        if (alias != null)
                        {
                            alias.Tracks.Add(track);
                        }
                        else
                        {
                            var aliasName = Regex.Replace(track.Name, @"[^\w\s]", "").ToLowerInvariant();
                            alias = new TrackAlias
                            {
                                Name = aliasName
                            };
                            alias.Tracks.Add(track);
                            await _dataContext.AddAsync(alias);
                        }
                        aliasCount++;
                        if (aliasCount == 50)
                        {
                            await _dataContext.SaveChangesAsync();
                            aliasCount = 0;
                        }
                    }
                    else
                    {
                            track.Alias.Name = Regex.Replace(track.Name, @"[^\w\s]", "").ToLowerInvariant();
                    }
                }
            }
            await _dataContext.SaveChangesAsync();
        }
    }
}

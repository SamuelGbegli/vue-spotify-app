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

        /// <summary>
        /// Automatically creates a set of aliases for tracks that do not have one.
        /// </summary>
        /// <returns></returns>
        public async Task GenerateAliases()
        {
            // The total number of new aliases created
            int aliasCount = 0;
            // The maximum number of tracks to be fetched from the database at one time
            const int trackBatchCount = 100;
            // Loops through track database in groups of 100
            for (int i = 0; i < await _dataContext.Tracks.Include(t => t.Alias).CountAsync(); i += trackBatchCount)
            {
                // Gets selected tracks from the database
                var tracks = await _dataContext.Tracks.Include(t => t.Alias).Skip(i).Take(trackBatchCount).ToListAsync();

                foreach (var track in tracks)
                {
                    // Section if track has no alias
                    if (track.AliasID == null)
                    {
                        // Gets the first alias that contains a track with a matching ISRC or matchkey
                        var alias = await _dataContext.TrackAliases.Include(a => a.Tracks).FirstOrDefaultAsync(a => a.Tracks.Any(t => t.ISRC == track.ISRC || (!string.IsNullOrWhiteSpace(t.MatchKey) && t.MatchKey == track.MatchKey)));
                        // Adds track to alias if one is found
                        if (alias != null)
                        {
                            alias.Tracks.Add(track);
                        }
                        // Creates and adds a new alias if one does not exist
                        else
                        {
                            var aliasName = Regex.Replace(track.Name, @"[^\w\s]", "").ToLowerInvariant();
                            alias = new TrackAlias
                            {
                                Name = aliasName,
                                PrimaryTrackID = track.ID
                            };
                            alias.Tracks.Add(track);
                            await _dataContext.AddAsync(alias);
                        }
                        // Increments number of aliases added by 1
                        aliasCount++;

                        // Saves database and resets alias count when 50 is reached
                        if (aliasCount == 50)
                        {
                            await _dataContext.SaveChangesAsync();
                            aliasCount = 0;
                        }
                    }
                    // Updates track alias name
                    else
                    {
                        track.Alias.Name = track.Name;
                    }
                }
            }

            // Ends by saving changes to the database
            await _dataContext.SaveChangesAsync();
        }
    
        public async Task UpdateAliasStatistics(User user, CancellationToken cancellationToken = default)
        {
            // The total number of new aliases created
            int aliasCount = 0;
            // The maximum number of tracks to be fetched from the database at one time
            const int batchCount = 100;

            for (int i = 0; i < await _dataContext.TrackAliases.CountAsync(); i += batchCount)
            {
                var aliases = await _dataContext.TrackAliases
                    .Include(ta => ta.Tracks)
                    .Include(ta => ta.Statistics)
                    .Skip(i)
                    .Take(batchCount)
                    .ToListAsync(cancellationToken);

                foreach(var alias in aliases)
                {
                    if(alias.Statistics == null)
                    {
                        alias.Statistics = new TrackAliasPlaybackStatistics
                        {
                            TrackAliasID = alias.ID,
                            UserID = user.ID
                        };
                        //_dataContext.TrackAliasPlaybackStatistics.Add(alias.Statistics);
                        
                    }
                    var trackPlays = _dataContext.PlaybackRecords
                            .Where(pr => pr.UserID == user.ID && alias.Tracks.Select(t => t.ID).Contains(pr.SpotifyID))
                            .Select(pr => pr.DatePlayed);

                    int totalPlays = await trackPlays.CountAsync(cancellationToken);
                    DateTime? latestPlay = totalPlays > 0 ? await trackPlays.MaxAsync(cancellationToken) : null;


                    if (alias.Statistics.TotalRecordedPlays != totalPlays || alias.Statistics.LastRecordedPlayDate != latestPlay)
                    {
                        alias.Statistics.TotalRecordedPlays = totalPlays;
                        alias.Statistics.LastRecordedPlayDate = latestPlay;
                        alias.Statistics.LastUpdated = DateTime.Now;
                    }

                    if (aliasCount < 50) aliasCount++;
                    else
                    {
                        await _dataContext.SaveChangesAsync();
                        aliasCount = 0;
                    }
                }

            }
            await _dataContext.SaveChangesAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Classes;
using vue_spotify_app.Classes.APIData;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Services
{
    public class SavedTrackService
    {
        private readonly DataContext _dataContext;
        private readonly TrackService _trackService;
        private readonly PlaybackRecordService _playbackRecordService;
        private readonly SpotifyAPIWrapper _spotifyAPIWrapper;

        public SavedTrackService(DataContext dataContext, TrackService trackService, PlaybackRecordService playbackRecordService, SpotifyAPIWrapper spotifyAPIWrapper)
        {
            _dataContext = dataContext;
            _trackService = trackService;
            _playbackRecordService = playbackRecordService;
            _spotifyAPIWrapper = spotifyAPIWrapper;
        }


        public async Task AddTracksToSavedTracks(User user, List<string> trackIds)
        {
            foreach (var trackId in trackIds)
            {
                var existingSavedTrack = await _dataContext.SavedTracks
                    .FirstOrDefaultAsync(t => t.UserID == user.ID && t.SpotifyID == trackId);
                if (existingSavedTrack == null)
                {
                    var savedTrack = new SavedTrack
                    {
                        UserID = user.ID,
                        SpotifyID = trackId,
                        DateAdded = DateTime.UtcNow
                    };
                    _dataContext.SavedTracks.Add(savedTrack);
                }

                // Section to cache track if not already cached
                if (await _dataContext.Tracks.FindAsync(trackId) == null)
                {
                    // Makes an API call with the track ID
                    var track = await _spotifyAPIWrapper.GetAsync<Classes.APIData.Track>(user.ID, $"tracks/{trackId}");
                    await _trackService.AddOrUpdateTrack(track);
                    await Task.Delay(50); // Delay to avoid hitting rate limits
                }
            }
            await _dataContext.SaveChangesAsync();
        }

        public async Task RemoveTracksFromSavedTracks(User user, List<string> trackIds)
        {
            foreach (var trackId in trackIds)
            {
                var savedTrack = await _dataContext.SavedTracks
                    .FirstOrDefaultAsync(t => t.UserID == user.ID && t.SpotifyID == trackId);
                if (savedTrack != null)
                {
                    _dataContext.SavedTracks.Remove(savedTrack);
                }
            }
            await _dataContext.SaveChangesAsync();
        }

        /// <summary>
        /// Returns a list of the user's saved tracks with pagination.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="offset">The offset for pagination.</param>
        /// <param name="count">The number of tracks to return.</param>
        /// <returns>A tuple containing the total number of tracks and the list of tracks.</returns>
        public async Task<(int, List<TrackViewModel>)> GetSavedTracks(User user, int offset = 0, int count = 50)
        {
            // Gets the number of tracks the user has saved
            var totalTracks = _dataContext.SavedTracks.Count(t => t.UserID == user.ID);
            // Selects tracks that the user has saved, with pagination offsets applied
            var savedTracks = await _dataContext.SavedTracks
                .Where(t => t.UserID == user.ID)
                .OrderByDescending(t => t.DateAdded)
                .Skip(offset * count)
                .Take(count)
                .ToListAsync();

            // List to store track view models
            var trackViewModels = new List<TrackViewModel>();

            // Gets the selected track's information from Spotify
            foreach (var savedTrack in savedTracks)
            {
                var track = await _trackService.GetTrack(user.ID, savedTrack.SpotifyID);
                if (track != null)
                {
                    track.DateSaved = savedTrack.DateAdded;
                    track.DateLastPlayed =  _playbackRecordService.GetPlaybackRecordsPerTrack(savedTrack.SpotifyID,1,1).Result.FirstOrDefault();

                    var cachedTrack = await _dataContext.Tracks.FindAsync(savedTrack.SpotifyID);
                    if (cachedTrack != null)
                    {
                        var aliasedTracks = await _dataContext.Tracks.Where(t => t.AliasID == cachedTrack.AliasID).ToListAsync();
                        track.IsInLikedSongs = _dataContext.TrackRecords.Any(
                            t => t.UserId == user.SpotifyUserID 
                            && aliasedTracks.Select(at => at.ID).Contains(t.SpotifyID)
                            && t.PlaylistID == null);
                    }

                    

                    trackViewModels.Add(track);
                }
            }
            return (totalTracks, trackViewModels);
        }

        /// <summary>
        /// Gets a random set of tracks from the user's saved tracks.
        /// </summary>
        /// <param name="userId">The ID of the logged in user.</param>
        /// <param name="count">The number of tracks to return.</param>
        /// <returns>A list of random tracks.</returns>
        public async Task<List<TrackViewModel>> GetRandomTracks(
            Guid userId,
            int count = 10
            )
        {
            // Stores the list of tracks to return
            var tracks = new List<TrackViewModel>();
            // Stores the indexes of the tracks to return from the database
            var indexesToReturn = new List<int>();
            // Gets the number of saved tracks for the user
            var numberOfSavedTracks = _dataContext.SavedTracks.Count(t => t.UserID == userId);
            // Creates a new random number generator
            var random = new Random();
            // Stores the maximum number of tracks to return based on whether the selected number or number of traxks is higher
            var max = count > numberOfSavedTracks ? numberOfSavedTracks : count;
            // Randomly chooses indexes to be returned
            while (indexesToReturn.Count < max)
            {
                var randomIndex = random.Next(numberOfSavedTracks);
                if (!indexesToReturn.Contains(randomIndex))
                {
                    indexesToReturn.Add(randomIndex);
                }
            }
            // Stores the selected saved tracks
            var savedTrackInfo = new List<SavedTrack>();

            // Queryable of the user's saved tracks ordered by date added
            var selectedTracks = _dataContext.SavedTracks
                .Where(testc => testc.UserID == userId)
                .OrderBy(testc => testc.DateAdded)
                .AsQueryable();

            // Gets items from the user's saved tracks based on the randomly selected indexes
            foreach (var index in indexesToReturn)
            {
                savedTrackInfo.Add(selectedTracks.ElementAt(index));
            }

            // Gets track information for each selected track
            foreach (var item in savedTrackInfo)
            {

                var track = await _trackService.GetTrack(userId,item.SpotifyID);
                if (track != null)
                {
                    track.DateSaved = item.DateAdded;
              
                    tracks.Add(track);
                }
            }

            return tracks;
        }
    }
}

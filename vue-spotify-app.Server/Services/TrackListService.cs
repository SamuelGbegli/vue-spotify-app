using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using vue_spotify_app.Classes;
using vue_spotify_app.Classes.APIData;
using vue_spotify_app.Classes.SortNameHelpers;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Services
{
    public class TrackListService
    {
        private readonly DataContext _dataContext;
        private readonly TrackService _trackService;
        private readonly SpotifyAPIWrapper _spotifyAPIWrapper;
        public TrackListService(DataContext dataContext, TrackService trackService, SpotifyAPIWrapper spotifyAPIWrapper)
        {
            _dataContext = dataContext;
            _trackService = trackService;
            _spotifyAPIWrapper = spotifyAPIWrapper;
        }

        public async Task<(int, List<TrackListViewModel>)> GetTrackLists(User user, int offset = 0, int numberOfLists = 20, CancellationToken cancellationToken = default)
        {
            var totalCount = await _dataContext.TrackLists
                .Where(tl => tl.UserID == user.SpotifyUserID && tl.TrackListType == TrackListType.InternalList)
                .CountAsync(cancellationToken);
            var trackLists = await _dataContext.TrackLists
                .Where(tl => tl.UserID == user.SpotifyUserID && tl.TrackListType == TrackListType.InternalList)
                .OrderByDescending(tl => tl.DateModified)
                .Skip(offset * numberOfLists)
                .Take(numberOfLists)
                .Select(tl => new TrackListViewModel
                {
                    ID = tl.ID,
                    Name = tl.Name,
                    DateCreated = tl.DateCreated,
                    DateModified = tl.DateModified
                })
                .ToListAsync(cancellationToken);
            return (totalCount, trackLists);
        }

        public async Task<TrackListViewModel> GetTrackList(User user, Guid listId, CancellationToken cancellationToken = default)
        {
            var trackList = await _dataContext.TrackLists
                .Where(tl => tl.UserID == user.SpotifyUserID && tl.ID == listId)
                .Select(tl => new TrackListViewModel
                {
                    ID = tl.ID,
                    Name = tl.Name,
                    DateCreated = tl.DateCreated,
                    DateModified = tl.DateModified
                })
                .FirstAsync(cancellationToken);
            return trackList;
        }

        public async Task AddTrackList(User user, string name, CancellationToken cancellationToken = default)
        {
            var trackList = new TrackList
            {
                UserID = user.SpotifyUserID,
                Name = name,
                SortName = RegexHelpers.GenerateSortName(name),
                TrackListType = TrackListType.InternalList,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow
            };
            _dataContext.TrackLists.Add(trackList);
            await _dataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateTrackListName(User user, Guid listId, string newName, CancellationToken cancellationToken = default)
        {
            var trackList = await _dataContext.TrackLists
                .Where(tl => tl.UserID == user.SpotifyUserID && tl.ID == listId)
                .FirstAsync(cancellationToken);
            if (trackList != null)
            {
                trackList.Name = newName;
                trackList.SortName = RegexHelpers.GenerateSortName(newName);
                trackList.DateModified = DateTime.UtcNow;
                await _dataContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteTrackList(User user, Guid listId, CancellationToken cancellationToken = default)
        {
            var trackList = await _dataContext.TrackLists
                .Where(tl => tl.UserID == user.SpotifyUserID && tl.ID == listId)
                .FirstAsync(cancellationToken);
            if (trackList != null)
            {
                _dataContext.TrackLists.Remove(trackList);
                await _dataContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task AddTracksToList(User user, Guid listId, List<string> trackIDs, CancellationToken cancellationToken = default)
        {
            var trackList = await _dataContext.TrackLists
                .Where(tl => tl.UserID == user.SpotifyUserID && tl.ID == listId)
                .FirstAsync(cancellationToken);
            if (trackList != null)
            {
                foreach (var trackID in trackIDs)
                {
                    if(_dataContext.TrackRecords.Any(tr => tr.TrackListID == listId && tr.SpotifyID == trackID)) continue; // Skip if the track is already in the list
                    else
                    {
                        if(await _dataContext.Tracks.FirstOrDefaultAsync(t => t.ID == trackID, cancellationToken) == null)
                        {
                            var track = await _spotifyAPIWrapper.GetAsync<Classes.APIData.Track>(user.ID, $"tracks/{trackID}");
                            await _trackService.AddOrUpdateTrack(track);
                            await Task.Delay(50); // Delay to avoid hitting rate limits
                        }
                    }
                    var trackRecord = new TrackRecord
                    {
                        UserId = user.SpotifyUserID,
                        TrackListID = listId,
                        SpotifyID = trackID,
                        DateAdded = DateTime.UtcNow
                    };
                    _dataContext.TrackRecords.Add(trackRecord);
                }
                trackList.DateModified = DateTime.UtcNow;
                await _dataContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RemoveTracksFromList(User user, Guid listId, List<string> trackIDs, CancellationToken cancellationToken = default)
        {
            var trackList = await _dataContext.TrackLists
                .Where(tl => tl.UserID == user.SpotifyUserID && tl.ID == listId)
                .FirstAsync(cancellationToken);
            if (trackList != null)
            {
                foreach (var trackID in trackIDs)
                {
                    var trackRecord = await _dataContext.TrackRecords
                        .Where(tr => tr.TrackListID == listId && tr.SpotifyID == trackID)
                        .FirstOrDefaultAsync(cancellationToken);
                    if (trackRecord != null)
                    {
                        _dataContext.TrackRecords.Remove(trackRecord);
                    }
                }
                trackList.DateModified = DateTime.UtcNow;
                await _dataContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

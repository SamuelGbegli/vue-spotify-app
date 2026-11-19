using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Classes;
using vue_spotify_app.Classes.APIData;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Services
{
    public class ExtendedRecordService
    {
        private readonly DataContext _dataContext;
        private readonly TrackService _trackService;
        private readonly PlaybackRecordService _playbackRecordService;
        private readonly SpotifyAPIWrapper _spotifyAPIWrapper;

        public ExtendedRecordService(DataContext dataContext, TrackService trackService, PlaybackRecordService playbackRecordService, SpotifyAPIWrapper spotifyAPIWrapper)
        {
            _dataContext = dataContext;
            _trackService = trackService;
            _playbackRecordService = playbackRecordService;
            _spotifyAPIWrapper = spotifyAPIWrapper;
        }

        public async Task<(int, List<ExtendedRecordViewModel>)> GetRecords(Guid userId, int offset, int numberOfRecords, CancellationToken cancellationToken = default)
        {

            offset = Math.Max(offset, 0);
            numberOfRecords = Math.Clamp(numberOfRecords, 1, 100);

            var totalCountTask = await _dataContext.ExtendedRecords.CountAsync();

            var query = _dataContext.ExtendedRecords
                .AsNoTracking()
                .OrderByDescending(r => r.Timestamp)
                .ThenBy(r => r.ID)
                .Select(record => new
                {
                    record.ID,
                    record.TrackURI,
                    record.AmountPlayed,
                    record.ReasonStart,
                    record.ReasonEnd,
                    record.Timestamp
                });

            var indexValue = query.ElementAt(offset);

            if (offset > 0)
                query = query.Where(r =>
                    r.Timestamp < indexValue.Timestamp ||
                    r.Timestamp == indexValue.Timestamp && r.ID.CompareTo(indexValue.ID) >= 0
                );

            var records = await query.Take(numberOfRecords).ToListAsync(cancellationToken);
            var trackIds = records
                .Select(record => record.TrackURI.Replace("spotify:track:", ""))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var tracks = new Dictionary<string, TrackViewModel?>(StringComparer.Ordinal);
            foreach (var trackId in trackIds)
                tracks[trackId] = await _trackService.GetTrack(userId, trackId);

            var viewModels = records.Select(record =>
            {
                var trackId = record.TrackURI.Replace("spotify:track:", "");
                return new ExtendedRecordViewModel
                {
                    ID = record.ID,
                    Track = tracks[trackId],
                    AmountPlayed = record.AmountPlayed,
                    ReasonStart = record.ReasonStart,
                    ReasonEnd = record.ReasonEnd,
                    Timestamp = record.Timestamp,
                };
            }).ToList();

            return (totalCountTask, viewModels);
        }
    }
}

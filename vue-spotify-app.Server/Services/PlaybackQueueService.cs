using vue_spotify_app.Classes.APIData;

namespace vue_spotify_app.Server.Services
{
    public class PlaybackQueueService
    {
        private readonly SpotifyAPIWrapper _spotifyAPIWrapper;
        public PlaybackQueueService(SpotifyAPIWrapper spotifyAPIWrapper)
        {
            _spotifyAPIWrapper = spotifyAPIWrapper;
        }
        public async Task<List<DeviceInfo>> GetUserDevices(Guid userId)
        {
            var response = await _spotifyAPIWrapper.GetAsync<DevicesResponse>(userId, "me/player/devices");
            return response.devices;
        }

        public async Task AddTrackToQueue(Guid userId, List<string> spotifyTrackIDs, string deviceId)
        {
            foreach (var trackID in spotifyTrackIDs)
            {
                await _spotifyAPIWrapper.PostAsync(userId, $"me/player/queue?uri=spotify:track:{trackID}&device_id={deviceId}", null);
                await Task.Delay(500); // Delay to avoid hitting rate limits
            }
        }
    }
}

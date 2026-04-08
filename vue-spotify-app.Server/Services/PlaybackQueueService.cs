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

        public async Task AddTrackToQueue(Guid userId, string spotifyTrackID, string deviceId)
        {
            await _spotifyAPIWrapper.PostAsync(userId, $"me/player/queue?uri=spotify:track:{spotifyTrackID}&device_id={deviceId}", null);
        }
    }
}

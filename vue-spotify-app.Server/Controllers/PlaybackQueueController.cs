using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Classes;
using vue_spotify_app.Server.Data;
using vue_spotify_app.Server.Services;

namespace vue_spotify_app.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlaybackQueueController: ControllerBase
    {
        private readonly PlaybackQueueService _playbackQueueService;
        private readonly DataContext _dataContext;

        public PlaybackQueueController(PlaybackQueueService playbackQueueService, DataContext dataContext)
        {
            _playbackQueueService = playbackQueueService;
            _dataContext = dataContext;
        }

        [HttpGet]
        [Route("getdevices")]
        public async Task<IActionResult> GetUserDevices()
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.SpotifyTokens.FirstAsync(t => t.ID.ToString() == userId);

                var devices = await _playbackQueueService.GetUserDevices(user.ID);
                return Ok(devices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching devices: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("addtoqueue")]
        public async Task<IActionResult> AddTrackToQueue([FromBody] AddToQueueDTO dto)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.SpotifyTokens.FirstAsync(t => t.ID.ToString() == userId);

                await _playbackQueueService.AddTrackToQueue(user.ID, dto.SpotifyTrackID, dto.DeviceID);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error adding track to queue: {ex.Message}");
            }
        }
    }
}

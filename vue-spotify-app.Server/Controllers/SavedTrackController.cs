using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vue_spotify_app.Server.Data;
using vue_spotify_app.Server.Services;

namespace vue_spotify_app.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SavedTrackController : ControllerBase
    {

        private readonly SavedTrackService _savedTrackService;
        private readonly DataContext _dataContext;

        public SavedTrackController(SavedTrackService savedTrackService, DataContext dataContext)
        {
            _savedTrackService = savedTrackService;
            _dataContext = dataContext;
        }
        [HttpPost]
        [Route("addtosavedtracks")]
        public async Task<IActionResult> AddToSavedTracks([FromBody] List<string> trackIds)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = _dataContext.Users.FirstOrDefault(u => u.ID.ToString() == userId);
                await _savedTrackService.AddTracksToSavedTracks(user, trackIds);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error adding tracks: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("removefromsavedtracks")]
        public async Task<IActionResult> RemoveFromSavedTracks([FromBody] List<string> trackIds)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = _dataContext.Users.FirstOrDefault(u => u.ID.ToString() == userId);
                await _savedTrackService.RemoveTracksFromSavedTracks(user, trackIds);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error removing tracks: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("getsavedtracks")]
        public async Task<IActionResult> GetSavedTracks([FromQuery] int offset = 0, [FromQuery] int count = 50)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = _dataContext.Users.FirstOrDefault(u => u.ID.ToString() == userId);

                var (totalTracks, tracks) = await _savedTrackService.GetSavedTracks(user, offset, count);
                return Ok(new { TotalTracks = totalTracks, Tracks = tracks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching tracks: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("getrandomsavedtracks")]
        public async Task<IActionResult> GetRandomSavedTracks([FromBody] int count = 10)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = _dataContext.SpotifyTokens.First(t => t.ID.ToString() == userId);

                var tracks = await _savedTrackService.GetRandomTracks(user.ID, count);
                return Ok(tracks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching tracks: {ex.Message}");
            }
        }
    }
}

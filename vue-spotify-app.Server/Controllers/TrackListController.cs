using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Server.Data;
using vue_spotify_app.Server.Migrations;
using vue_spotify_app.Server.Services;

namespace vue_spotify_app.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TrackListController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly TrackListService _trackListService;

        public TrackListController(DataContext dataContext, TrackListService trackListService)
        {
            _dataContext = dataContext;
            _trackListService = trackListService;
        }

        [HttpGet]
        [Route("gettracklists")]
        public async Task<IActionResult> GetTrackLists([FromQuery] int offset = 0, [FromQuery] int numberOfLists = 20)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);

                var (count, trackLists) = await _trackListService.GetTrackLists(user, offset, numberOfLists);
                return Ok(new
                {
                    count,
                    trackLists
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching track lists: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("gettracklist/{id}")]
        public async Task<IActionResult> GetTrackList([FromRoute] Guid id)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);

                var trackList = await _trackListService.GetTrackList(user, id);
                return Ok(trackList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching track list: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("addtracklist")]
        public async Task<IActionResult> AddTrackList([FromBody] string name)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);

                await _trackListService.AddTrackList(user, name);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error adding track list: {ex.Message}");
            }
        }

        [HttpPatch]
        [Route("updatetracklistname/{id}")]
        public async Task<IActionResult> UpdateTrackListName([FromRoute] Guid id, [FromBody] string newName)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);

                await _trackListService.UpdateTrackListName(user, id, newName);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating track list name: {ex.Message}");
            }
        }

        [HttpDelete]
        [Route("deletetracklist/{id}")]
        public async Task<IActionResult> DeleteTrackList([FromRoute] Guid id)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);

                await _trackListService.DeleteTrackList(user, id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting track list: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("addtrackstolist")]
        public async Task<IActionResult> AddTracksToList([FromBody] TrackListDTO dto)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);
                await _trackListService.AddTracksToList(user, dto.ListID, dto.TrackIDs);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error adding tracks to list: {ex.Message}");
            }
        }

        [HttpDelete]
        [Route("removetracksfromlist")]
        public async Task<IActionResult> RemoveTracksFromList([FromBody] TrackListDTO dto)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);

                await _trackListService.RemoveTracksFromList(user, dto.ListID, dto.TrackIDs);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error removing tracks from list: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("validatelistname")]
        public async Task<IActionResult> ValidateTrackName([FromBody] string listName)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);

                if (listName.Length < 1) return Ok("List name cannot be empty.");
                if (string.IsNullOrWhiteSpace(listName)) return Ok("List name is invalid.");

                var result = await _dataContext.TrackLists.CountAsync(tr => tr.TrackListType == Classes.TrackListType.InternalList
                    && tr.UserID == user.SpotifyUserID
                    && tr.Name.ToLower() == listName.ToLower());

                if (result != 0) return Ok("A list with that name already exists.");

                return Ok("");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error validating name: {ex.Message}");
            }
        }
    }
}

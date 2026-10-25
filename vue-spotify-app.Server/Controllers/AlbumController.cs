using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Server.Data;
using vue_spotify_app.Server.Services;

namespace vue_spotify_app.Server.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AlbumController : ControllerBase
    {
        private readonly AlbumService _albumService;
        private readonly DataContext _dataContext;

        public AlbumController(AlbumService albumService, DataContext context)
        {
            _albumService = albumService;
            _dataContext = context;
        }

        [HttpGet("getalbumwithtracks/{albumID}")]
        public async Task<IActionResult> GetAlbumWithTracks([FromRoute] string albumID)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);

                var (album, tracks) = await _albumService.GetAlbumWithTracks(user, albumID);
                return Ok(new { Album = album, Tracks = tracks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlaylistController : ControllerBase
    {
        private readonly PlaylistService _playlistService;
        private readonly DataContext _dataContext;

        public PlaylistController(PlaylistService playlistService, DataContext dataContext)
        {
            _playlistService = playlistService;
            _dataContext = dataContext;
        }

        [HttpGet]
        [Route("getplaylists")]
        public async Task<IActionResult> GetPlaylists([FromHeader] string authToken, int offset = 0, int numberOfPlaylists = 0)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);

                var data = await _playlistService.GetPlaylists(user.ID, offset, numberOfPlaylists);

                return Ok(new
                {
                    totalPlaylists = data.Item1,
                    playlists = data.Item2
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("getplaylist/{id}")]
        public async Task<IActionResult> GetPlaylist([FromRoute] string id)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);
                var playlist = await _playlistService.GetPlaylist(user.ID, id);
                if (playlist == null)
                    return NotFound();
                return Ok(playlist);
            }
            catch (Exception ex)
            {
                 return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("getplaylisttracks/{id}")]
        public async Task<IActionResult> GetPlaylistTracks([FromRoute] string id, [FromHeader] string authToken, int offset = 0, int numberOfTracks = 0)
        {
            try
            {
                var tracks = await _playlistService.GetPlaylistTracks(id, "", Classes.TrackSortType.Name, Classes.SortOrder.Ascending, offset, numberOfTracks);
                return Ok(tracks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("gettrackplaylists")]
        public async Task<IActionResult> GetTrackPlaylists([FromHeader] string authToken, [FromQuery] string trackId, [FromQuery] int offset = 0, [FromQuery] int numberOfPlaylists = 10)
        {
            try
            {
                var totalPlaylists = await _playlistService.GetNumberOfTrackPlaylists(trackId);
                var playlists = await _playlistService.GetPlaylistsPerTrack(trackId, authToken, offset, numberOfPlaylists);
                return Ok(new
                {
                    totalPlaylists,
                    playlists
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("initialisetracks")]
        public async Task<IActionResult> InitialiseTracks([FromHeader] string authToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                //await _playlistService.InitialisePlaylist(authToken, 0, 20);
                stopwatch.Stop();
                return Ok(stopwatch.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return StatusCode(500, ex.Message);
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http.Formatting;
using System.Text.Json;
using System.Threading.Tasks;
using vue_spotify_app.Classes;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrackController : ControllerBase
    {
        private readonly TrackService _trackService;
        private readonly PlaybackRecordService _playbackRecordService;
        private readonly DataContext _dataContext;

        public TrackController(TrackService trackService, PlaybackRecordService playbackRecordService, DataContext dataContext)
        {
            _trackService = trackService;
            _playbackRecordService = playbackRecordService;
            _dataContext = dataContext;
        }

        [HttpPost]
        [Route("testpost")]
        public ActionResult TestPost(Track track)
        {
            try
            {
                return Ok(track.Name);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("makecalltoapi")]
        public async Task<ActionResult> MakeCallToApi([FromQuery] string authToken, [FromQuery] string trackId = "0NYsD6fsdx5uoghZwnaLkX")
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

                HttpResponseMessage responseMessage = client.GetAsync($"https://api.spotify.com/v1/tracks/{trackId}").Result;
                if (responseMessage.IsSuccessStatusCode)
                {
                    using var contentStream = await responseMessage.Content.ReadAsStreamAsync();
                    var item = await JsonSerializer.DeserializeAsync<vue_spotify_app.Classes.APIData.Track>(contentStream);

                    return Ok(item.name);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("gettracks")]
        [Authorize]
        public async Task<IActionResult> GetTracks(
            [FromQuery] List<int> offset,
            [FromQuery] string? playlistId = null,
            [FromQuery] string query = "",
            [FromQuery] bool searchName = true,
            [FromQuery] bool searchArtist = true,
            [FromQuery] bool searchAlbum = true,
            [FromQuery] DateTime? dateRangeFrom = null,
            [FromQuery] DateTime? dateRangeTo = null,
            [FromQuery] SortType sortType = SortType.Name,
            [FromQuery] Classes.SortOrder sortOrder = Classes.SortOrder.Ascending,
            [FromQuery] int numberOfTracks = 10)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var filter = new TrackFilter
                {
                    Query = query,
                    SearchName = searchName,
                    SearchArtist = searchArtist,
                    SearchAlbum = searchAlbum,
                    DateRangeFrom = dateRangeFrom,
                    DateRangeTo = dateRangeTo,
                    SortType = sortType,
                    SortOrder = sortOrder
                };

                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);

                var data = await _trackService.GetTracksNew(
                    spotifyUserID: user.SpotifyUserID,
                     playlistId: playlistId,
                     filter: filter,
                     offsets: offset,
                     numberOfTracks: numberOfTracks);

                stopwatch.Stop();
                return Ok(new
                {
                    timeElapsed = stopwatch.Elapsed,
                    totalTracks =data.Item1,
                    tracks = data.Item2
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("testplaybackhistory")]
        public async Task<IActionResult> TestPlaybackHistory([FromHeader] string authToken)
        {
            await _playbackRecordService.UpdatePlaybackHistory(authToken);
            return Ok();
        }

        [HttpGet]
        [Route("initialisetracks")]
        public async Task<IActionResult> InitialiseTracks([FromHeader] string authToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);
                await _trackService.InitialiseTracks(user.ID);
                stopwatch.Stop();
                return Ok(stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("gettrack/{id}")]
        public async Task<IActionResult> GetTrack([FromHeader] string authToken, [FromRoute] string id)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);
                var viewModel = await _trackService.GetTrack(user.ID, id);
                return Ok(viewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("exporttracks")]
        public async Task<IActionResult> ExportTracks()
        {
            // TODO: rewrite function to create and return CSV file
            try
            {
                //var tracks = await _trackService.GetTracks();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

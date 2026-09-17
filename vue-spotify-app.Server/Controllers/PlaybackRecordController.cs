using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Classes;
using vue_spotify_app.Classes.APIData;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaybackRecordController : ControllerBase
    {

        private readonly DataContext _dataContext;
        private readonly PlaybackRecordService _playbackRecordService;

        public PlaybackRecordController(DataContext dataContext, PlaybackRecordService playbackRecordService)
        {
            _dataContext = dataContext;
            _playbackRecordService = playbackRecordService;
        }

        [HttpGet]
        [Route("getpendingrecords")]
        public async Task<IActionResult> GetPendingRecords([FromQuery] int page)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var spotifyToken = await _dataContext.SpotifyTokens.FirstAsync(t => t.ID.ToString() == userId);

                var totalRecords = await _dataContext.PendingPlaybackRecords.CountAsync();
                var records = await _dataContext.PendingPlaybackRecords
                    .OrderBy(r => r.DateRecorded)
                    .Skip((page - 1) * 50).Take(50).ToListAsync();

                var recordViewModels = new List<PendingPlaybackRecordViewModel>();

                // Sets up HTTP Client
                HttpClient client = new HttpClient();
                // Adds JSON header to client
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                // Adds Spotify auth token to header
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", spotifyToken.AccessToken);

                foreach (var record in records)
                {

                    HttpResponseMessage responseMessage = await client.GetAsync($"https://api.spotify.com/v1/tracks/{record.InputtedSpotifyID}");
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        using var contentStream = responseMessage.Content.ReadAsStream();
                        // Deserialize JSON response to object
                        var trackInfo = System.Text.Json.JsonSerializer.Deserialize<Classes.APIData.Track>(contentStream);
                        var recordViewModel = new PendingPlaybackRecordViewModel
                        {
                            ID = record.ID,
                            DateAdded = record.DateRecorded,
                            RecordedTrackName = record.InputtedName,
                            RecordedSpotifyID = record.InputtedSpotifyID,
                            FoundAlbum = trackInfo.album.name,
                            FoundAlbumCover = trackInfo.album.images.FirstOrDefault().url,
                            FoundTrackName = trackInfo.name,
                        };
                        // Adds artist names to view model
                        foreach (var artist in trackInfo.artists)
                        {
                            recordViewModel.FoundArtists.Add(new Classes.Artist
                            {
                                Name = artist.name,
                            });
                        }
                        recordViewModels.Add(recordViewModel);

                    }
                    Thread.Sleep(20);
                }
                return Ok(new
                {
                    totalRecords,
                    records = recordViewModels
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("updaterecords")]
        public async Task<IActionResult> UpdateRecords()
        {
            try
            {

                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var authToken = await _dataContext.SpotifyTokens.FirstAsync(t => t.ID.ToString() == userId);
                await _playbackRecordService.UpdatePlaybackHistory(authToken.AccessToken);
                return Ok();
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("getrecords")]
        public async Task<IActionResult> GetRecords(
            //[FromHeader] string authToken,
            [FromQuery] int offset,
            [FromQuery] int numberOfRecords,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);
                var data = await _playbackRecordService.GetPlaybackRecords(user, offset, numberOfRecords, startDate, endDate);
                stopwatch.Stop();
                return Ok(new
                {
                    elapsedTime = stopwatch.ElapsedMilliseconds,
                    records = data.Item2,
                    totalRecords = data.Item1
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Moves a set of pending playback records to the main playback database.
        /// </summary>
        /// <param name="pendingRecordIDs"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("pushpendingrecords")]
        public async Task<IActionResult> PushPendingRecords([FromBody] List<string> pendingRecordIDs)
        {
            try
            {
                await _playbackRecordService.PushPendingPlaybackRecords(pendingRecordIDs);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);

            }
        }

        [HttpGet]
        [Route("getrecordspertrack")]
        public async Task<IActionResult> GetRecordsPerTrack([FromQuery] string trackId, [FromQuery] int offset = 1, [FromQuery] int numberOfRecords = 20)
        {
            try
            {
                var records = await _playbackRecordService.GetPlaybackRecordsPerTrack(trackId, offset, numberOfRecords);
                var totalRecords = await _playbackRecordService.GetNumberOfTrackRecords(trackId);
                return Ok(new
                {
                    records,
                    totalRecords
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Route("gettrackfoundrecords")]
        public async Task<IActionResult> GetTracksByPlaybackRecordCount([FromQuery] int offset = 0, [FromQuery] int numberOfTracks = 50, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);
                var records = await _playbackRecordService.GetTracksGroupedByPlaybackRecordsV2(user.ID, offset, numberOfTracks, startDate, endDate);
                return Ok(new
                {
                    totalRecords = records.Item1,
                    records = records.Item2
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Classes;
using vue_spotify_app.Classes.APIData;
using vue_spotify_app.Server.Data;

namespace vue_spotify_app.Server.Controllers
{
    [ApiController]
    [Authorize]
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
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);
                var data = await _playbackRecordService.GetPlaybackRecords(user, offset, numberOfRecords, startDate, endDate);
                stopwatch.Stop();
                return Ok(new
                {
                    elapsedTime = stopwatch.Elapsed,
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
        public async Task<IActionResult> GetGroupedPlaybackRecords([FromQuery] int offset = 0, [FromQuery] int numberOfTracks = 50, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);
                var records = await _playbackRecordService.GetTracksGroupedByPlaybackRecords(user.ID, offset, numberOfTracks, startDate, endDate);
                stopwatch.Stop();
                return Ok(new
                {
                    elapsedTime = stopwatch.Elapsed,
                    totalRecords = records.Item1,
                    records = records.Item2
                });
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("exporttop100")]
        public async Task<IActionResult> ExportListeningReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);

                var data = await _playbackRecordService.GeneratePlaybackReport(user, startDate, endDate);
                var fileName = $"Spotify data analysis ${DateTime.UtcNow:yyyy:MMM:dd}";

                return File(
                    data,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                    );

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

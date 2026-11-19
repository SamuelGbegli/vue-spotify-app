using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using vue_spotify_app.Server.Data;
using vue_spotify_app.Server.Services;

namespace vue_spotify_app.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExtendedRecordController : ControllerBase
    {

        private readonly ExtendedRecordService _extendedRecordService;
        private readonly DataContext _dataContext;

        public ExtendedRecordController(ExtendedRecordService extendedRecordService, DataContext dataContext)
        {
            _extendedRecordService = extendedRecordService;
            _dataContext = dataContext;
        }
        [HttpGet("GetRecords")]
        public async Task<IActionResult> GetRecords([FromQuery] int offset, [FromQuery] int numberOfRecords)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value;
                var user = await _dataContext.Users.FirstOrDefaultAsync(u => u.ID.ToString() == userId);

                var data = await _extendedRecordService.GetRecords(user.ID, offset, numberOfRecords);

                return Ok(new
                {
                    totalCount = data.Item1,
                    records = data.Item2
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

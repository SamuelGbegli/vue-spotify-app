using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vue_spotify_app.Classes;

namespace vue_spotify_app.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> Search([FromBody] SearchDTO searchDTO)
        {
            try
            {
                return Ok(searchDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

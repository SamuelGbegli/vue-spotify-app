using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace vue_spotify_app.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }
        [HttpGet("redirect")]
        public IActionResult GetRedirectURL()
        {
            var redirectURL = _authService.GenerateRedirectURL(out string state, out string codeVerifier);
            return Ok(redirectURL);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                return BadRequest("Missing code or state.");
            }
            bool getToken = false;
            var token = await _authService.GetToken(code, state, getToken);
            if (token == null && getToken)
            {
                return BadRequest("Failed to exchange code for token.");
            }
            return Ok();
        }

        [HttpGet]
        [Authorize]
        [Route("me")]
        public IActionResult GetMe()
        {
            try
            {
                return Ok(new
                {
                    userID = User.Claims.FirstOrDefault(c => c.Type == "spotify_id")?.Value,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _authService.Logout();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}

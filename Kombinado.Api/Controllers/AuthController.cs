using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Kombinado.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequestDto request)
        {
            var response = await _authService.SignupAsync(request);
            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }
    }
}

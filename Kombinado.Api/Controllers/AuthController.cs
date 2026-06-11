using Kombinado.Api.Extensions;
using Kombinado.Api.Models.DTOs;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Services.Auth;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var response = await _authService.LoginAsync(request);
            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            var response = await _authService.RefreshTokenAsync(request);
            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response);
            }

            return Ok(response);
        }
        
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            Guid userId = User.GetUserId();
            
            var response = await _authService.GetUserProfileAsync(userId);
            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response);
            }
            
            return Ok(response);
        }
    }
}

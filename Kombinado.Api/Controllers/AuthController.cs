using Kombinado.Api.Models.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Kombinado.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupRequestDto request)
        {
            return Ok("Por enquanto ainda não tem, mas ta chegando rapaze!");
        }
    }
}

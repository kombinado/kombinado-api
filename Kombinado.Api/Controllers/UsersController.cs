using Kombinado.Api.Extensions;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kombinado.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
    {
        Guid userId = User.GetUserId();

        var response = await _userService.UpdateProfileAsync(userId, request);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, response);
        }

        return Ok(response);
    }
}

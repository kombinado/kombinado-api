using Kombinado.Api.Extensions;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Services.RideRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kombinado.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/requests")]
public class RideRequestController : ControllerBase
{
    private readonly IRideRequestService _rideRequestService;
    public RideRequestController(IRideRequestService rideRequestService)
    {
        _rideRequestService = rideRequestService;
    }

    [HttpPost("ride/{rideId}")]
    public async Task<IActionResult> RequestSeat(Guid rideId, [FromBody] CreateRideRequestDto dto)
    {
        Guid passengerId = User.GetUserId();
        
        var response = await _rideRequestService.RequestSeatAsync(rideId, passengerId, dto);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, response);
        }
        
        return Ok(response);
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetMyRequests()
    {
        Guid passengerId = User.GetUserId();
        
        var response = await _rideRequestService.GetMyRequestsAsync(passengerId);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, response);
        }
        
        return Ok(response);
    }
    
    [HttpPatch("{requestId}/cancel")]
    public async Task<IActionResult> CancelRequest(Guid requestId)
    {
        Guid passengerId = User.GetUserId();
        
        var response = await _rideRequestService.CancelRequestAsync(requestId, passengerId);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, response);
        }
        
        return Ok(response);
    }
}
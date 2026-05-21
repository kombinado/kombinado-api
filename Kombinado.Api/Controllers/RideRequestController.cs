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
    
    [HttpGet("ride/{rideId}")]
    [Authorize(Policy = "DriverOnly")]
    public async Task<IActionResult> GetRequestsByRide(Guid rideId)
    {
        Guid driverId = User.GetUserId();
    
        var response = await _rideRequestService.GetRequestsByRideAsync(rideId, driverId);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, response);
        }
        
        return StatusCode(200, response); 
    }
    
    [HttpPatch("{requestId}/respond")]
    [Authorize(Policy = "DriverOnly")]
    public async Task<IActionResult> RespondToRequest(Guid requestId, [FromBody] RespondRequestDto dto)
    {
        Guid driverId = User.GetUserId();
    
        var response = await _rideRequestService.RespondToRequestAsync(requestId, driverId, dto.Accept);
        if (!response.Success)
        {
            return StatusCode(response.StatusCode, response);
        }

        return StatusCode(200, response);
    }
}
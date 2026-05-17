using Kombinado.Api.Extensions;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Services.Ride;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kombinado.Api.Controllers
{
    [Authorize] 
    [ApiController]
    [Route("api/[controller]")]
    public class RidesController : ControllerBase
    {
        private readonly IRideService _rideService;
        public RidesController(IRideService rideService)
        {
            _rideService = rideService;
        }

        [HttpPost]
        [Authorize("DriverOnly")]
        public async Task<IActionResult> CreateRide([FromBody] CreateRideRequestDto request)
        {
            Guid driverId = User.GetUserId();
            
            var response = await _rideService.CreateRideAsync(request, driverId);
            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response);
            }
            
            return StatusCode(201, response);
        }
        
        [HttpGet("me/driving")]
        [Authorize("DriverOnly")]
        public async Task<IActionResult> GetMyDrivingRides()
        {
            Guid driverId = User.GetUserId();
            
            var response = await _rideService.GetMyDrivingRidesAsync(driverId);
            if (!response.Success)
            {
                return StatusCode(response.StatusCode, response);
            }
            
            return Ok(response);
        }
    }
}
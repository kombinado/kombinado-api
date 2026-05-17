using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Models.DTOs.Responses;

namespace Kombinado.Api.Services.Ride
{
    public interface IRideService
    {
        public Task<ApiResponse<RideResponseDto>> CreateRideAsync(CreateRideRequestDto requestDto, Guid driverId);
    }
}
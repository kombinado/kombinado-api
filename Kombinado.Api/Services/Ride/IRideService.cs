using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Models.DTOs.Responses;

namespace Kombinado.Api.Services.Ride
{
    public interface IRideService
    {
        Task<ApiResponse<RideResponseDto>> CreateRideAsync(CreateRideRequestDto requestDto, Guid driverId);
        Task<ApiResponse<IEnumerable<RideResponseDto>>> GetAvailableRidesAsync(Guid currentUserId);
        Task<ApiResponse<IEnumerable<RideResponseDto>>> GetMyDrivingRidesAsync(Guid driverId);
        Task<ApiResponse<string>> CancelRideAsync(Guid rideId, Guid driverId);
    }
}
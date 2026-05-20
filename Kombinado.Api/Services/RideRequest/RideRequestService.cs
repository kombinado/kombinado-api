using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Models.DTOs.Responses;

namespace Kombinado.Api.Services.RideRequest;

public class RideRequestService : IRideRequestService
{
    public Task<ApiResponse<RideRequestResponseDto>> RequestSeatAsync(Guid rideId, Guid passengerId, CreateRideRequestDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<IEnumerable<RideRequestResponseDto>>> GetMyRequestsAsync(Guid passengerId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<string>> CancelRequestAsync(Guid requestId, Guid passengerId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<IEnumerable<RideRequestResponseDto>>> GetRequestsByRideAsync(Guid rideId, Guid driverId)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<string>> RespondToRequestAsync(Guid requestId, Guid driverId, bool accept)
    {
        throw new NotImplementedException();
    }
}
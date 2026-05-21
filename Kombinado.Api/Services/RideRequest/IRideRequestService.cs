using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Models.DTOs.Responses;

namespace Kombinado.Api.Services.RideRequest;

public interface IRideRequestService
{
    /// <summary>
    /// Create a new ride request for an open ride.
    /// </summary>
    Task<ApiResponse<RideRequestResponseDto>> RequestSeatAsync(Guid rideId, Guid passengerId, CreateRideRequestDto dto);

    /// <summary>
    /// List all ride requests made by the passenger (My Requests Panel).
    /// </summary>
    Task<ApiResponse<IEnumerable<RideRequestResponseDto>>> GetMyRequestsAsync(Guid passengerId);

    /// <summary>
    /// Allow passengers to cancel a pending/accepted ride request.
    /// </summary>
    Task<ApiResponse<string>> CancelRequestAsync(Guid requestId, Guid passengerId);
    
    /// <summary>
    /// List all ride requests for a specific ride (Driver Panel).
    /// </summary>
    Task<ApiResponse<IEnumerable<RideRequestResponseDto>>> GetRequestsByRideAsync(Guid rideId, Guid driverId);

    /// <summary>
    /// Driver can accept or reject a ride request.
    /// </summary>
    Task<ApiResponse<string>> RespondToRequestAsync(Guid requestId, Guid driverId, bool accept);
}
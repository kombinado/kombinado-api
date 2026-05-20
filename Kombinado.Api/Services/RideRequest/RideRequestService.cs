using Kombinado.Api.Constants;
using Kombinado.Api.Data;
using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Models.DTOs.Responses;
using Kombinado.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kombinado.Api.Services.RideRequest;

public class RideRequestService : IRideRequestService
{
    private readonly KombinadoDbContext _dbContext;
    public RideRequestService(KombinadoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<RideRequestResponseDto>> RequestSeatAsync(Guid rideId, Guid passengerId, CreateRideRequestDto dto)
    {
        RideEntity? ride = await _dbContext.Rides.FirstOrDefaultAsync(r => r.Id == rideId);
        if (ride == null)
        {
            return ApiResponse<RideRequestResponseDto>.FailureResponse("Carona não encontrada.", 404);
        }

        if (ride.Status != RideStatus.Open || ride.AvailableSeats <= 0)
        {
            return ApiResponse<RideRequestResponseDto>.FailureResponse(
                "Carona não está disponível para solicitações.", 
                400
            );
        }
        
        if (ride.DriverId == passengerId)
        {
            return ApiResponse<RideRequestResponseDto>.FailureResponse(
                "Você não pode solicitar vaga em sua própria carona.", 
                400
            );
        }
        
        bool alreadyRequested = await _dbContext.RideRequests
            .AnyAsync(rr => rr.RideId == rideId && 
                            rr.PassengerId == passengerId && 
                            rr.Status != RideRequestStatus.Rejected);
        if (alreadyRequested)
        {
            return ApiResponse<RideRequestResponseDto>.FailureResponse(
                "Você já solicitou uma vaga para esta carona.", 
                400
            );
        }
        
        RideRequestEntity newRequest = new RideRequestEntity
        {
            Id = Guid.NewGuid(),
            RideId = rideId,
            PassengerId = passengerId,
            Status = RideRequestStatus.Pending,
            MeetingPointSuggestion = dto.MeetingPointSuggestion,
            RequestDate = DateTime.UtcNow
        };
        
        _dbContext.RideRequests.Add(newRequest);
        await _dbContext.SaveChangesAsync();
        
        string? passagerName = await _dbContext.Users
            .Where(u => u.Id == passengerId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync();

        RideRequestResponseDto response = new RideRequestResponseDto
        {
            Id = newRequest.Id,
            Status = newRequest.Status,
            MeetingPointSuggestion = newRequest.MeetingPointSuggestion,
            PassengerName = passagerName,
            PhoneNumber = null
        };
        
        return ApiResponse<RideRequestResponseDto>.SuccessResponse(
            "Solicitação de vaga criada com sucesso. Aguarde a resposta do motorista.",
            response
        );
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
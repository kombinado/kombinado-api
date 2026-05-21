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

    public async Task<ApiResponse<IEnumerable<RideRequestResponseDto>>> GetMyRequestsAsync(Guid passengerId)
    {
        List<RideRequestResponseDto> responseList = await (
            from request in _dbContext.RideRequests
            where request.PassengerId == passengerId
        
            join ride in _dbContext.Rides on request.RideId equals ride.Id
            join driver in _dbContext.Users on ride.DriverId equals driver.Id
        
            select new RideRequestResponseDto
            {
                Id = request.Id,
                Status = request.Status,
                MeetingPointSuggestion = request.MeetingPointSuggestion,
                PassengerName = driver.Name, 
                PhoneNumber = null 
            }
        ).ToListAsync();
        
        return ApiResponse<IEnumerable<RideRequestResponseDto>>.SuccessResponse(
            "Solicitações de vaga recuperadas com sucesso.",
            responseList
        );
    }

    public async Task<ApiResponse<string>> CancelRequestAsync(Guid requestId, Guid passengerId)
    {
        RideRequestEntity? request = await _dbContext.RideRequests
            .Include(rr => rr.Ride)
            .FirstOrDefaultAsync(rr => rr.Id == requestId && rr.PassengerId == passengerId);
        
        if (request == null)
        {
            return ApiResponse<string>.FailureResponse("Solicitação de vaga não encontrada.", 404);
        }
        
        if (request.Status == RideRequestStatus.Rejected)
        {
            return ApiResponse<string>.FailureResponse(
                "Não é possível cancelar uma solicitação que já foi rejeitada pelo motorista.", 
                400
            );
        }

        if (request.Status == RideRequestStatus.Accepted && 
            request.Ride.DepartureTime.ToLocalTime() - DateTime.UtcNow < TimeSpan.FromMinutes(15))
        {
            return ApiResponse<string>.FailureResponse(
                "Não é possível cancelar uma solicitação aceita a menos de 15 minutos do horário de partida.", 
                400
            );
        }
        
        if (request.Status == RideRequestStatus.Accepted)
        {
            request.Ride.AvailableSeats += 1;
            request.Ride.Status = request.Ride.Status == RideStatus.Full ? RideStatus.Open : request.Ride.Status;
        }
        
        request.Status = RideRequestStatus.Cancelled;
        await _dbContext.SaveChangesAsync();
        
        return ApiResponse<string>.SuccessResponse("Solicitação de vaga cancelada com sucesso.");
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
using Kombinado.Api.Constants;
using Kombinado.Api.Data;
using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Models.DTOs.Responses;
using Kombinado.Api.Models.Entities;
using Kombinado.Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Kombinado.Api.Services.Ride;

public class RideService : IRideService
{
    private readonly KombinadoDbContext _dbContext;
    public RideService(KombinadoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<RideResponseDto>> CreateRideAsync(CreateRideDto dto, Guid driverId)
    {
        // 1. Normalize the departure time to UTC and reject past dates
        DateTime departureTimeUtc = DateTimeUtils.ToUtc(dto.DepartureTime);
        if (departureTimeUtc <= DateTime.UtcNow)
        {
            return ApiResponse<RideResponseDto>.FailureResponse("O horário de partida deve ser uma data futura.", 400);
        }

        string? driverName = await _dbContext.Users
            .Where(u => u.Id == driverId)
            .Select(u => u.Name)
            .FirstOrDefaultAsync();

        if (driverName == null)
        {
            return ApiResponse<RideResponseDto>.FailureResponse("Motorista não encontrado.", 404);
        }

        // 2. Create a ride
        RideEntity newRide = new RideEntity
        {
            Id = Guid.NewGuid(),
            DriverId = driverId,
            Origin = dto.Origin,
            Destination = dto.Destination,
            DepartureTime = departureTimeUtc,
            AvailableSeats = dto.TotalSeats,
            TotalSeats = dto.TotalSeats,
            Status = RideStatus.Open
        };
        
        // 3. Save the ride in DB
        _dbContext.Rides.Add(newRide);
        await _dbContext.SaveChangesAsync();
        
        // 4. Return the Ride response
        RideResponseDto responseDto = new RideResponseDto
        {
            Id = newRide.Id,
            DriverName = driverName,
            Origin = newRide.Origin,
            Destination = newRide.Destination,
            DepartureTime = newRide.DepartureTime,
            AvailableSeats = newRide.AvailableSeats,
            TotalSeats = newRide.TotalSeats,
            Status = newRide.Status
        };
        
        return ApiResponse<RideResponseDto>.SuccessResponse("Carona criada com sucesso.", responseDto);
    }

    public async Task<ApiResponse<IEnumerable<RideResponseDto>>> GetAvailableRidesAsync(Guid currentUserId)
    {
        List<RideEntity> availableRides = await _dbContext.Rides
            .Include(r => r.Driver)
            .Where(r => r.Status == RideStatus.Open &&
                        r.AvailableSeats > 0 &&
                        r.DepartureTime > DateTime.UtcNow &&
                        r.DriverId != currentUserId)
            .OrderBy(r => r.DepartureTime)
            .ToListAsync();
        
        List<RideResponseDto> responseDtos = availableRides.Select(r => new RideResponseDto
        {
            Id = r.Id,
            DriverName = r.Driver.Name,
            Origin = r.Origin,
            Destination = r.Destination,
            DepartureTime = r.DepartureTime,
            AvailableSeats = r.AvailableSeats,
            TotalSeats = r.TotalSeats,
            Status = r.Status,
            VehicleModel = r.Driver.VehicleModel,
            VehicleColor = r.Driver.VehicleColor,
            VehiclePlate = r.Driver.VehiclePlate
        }).ToList();
        
        return ApiResponse<IEnumerable<RideResponseDto>>.SuccessResponse(
            "Caronas disponíveis recuperadas com sucesso",
            responseDtos
        );
    }

    public async Task<ApiResponse<IEnumerable<RideResponseDto>>> GetMyDrivingRidesAsync(Guid driverId)
    {
        List<RideEntity> rides = await _dbContext.Rides
            .Include(r => r.Driver)
            .Include(r => r.Requests)
            .Where(r => r.DriverId == driverId)
            .ToListAsync();

        List<RideResponseDto> responseDtos = rides.Select(r => new RideResponseDto
        {
            Id = r.Id,
            DriverName = r.Driver.Name,
            Origin = r.Origin,
            Destination = r.Destination,
            DepartureTime = r.DepartureTime,
            AvailableSeats = r.AvailableSeats,
            TotalSeats = r.TotalSeats,
            Status = r.Status,
            PendingRequestsCount = r.Requests.Count(req => req.Status == RideRequestStatus.Pending)
        }).ToList();

        return ApiResponse<IEnumerable<RideResponseDto>>.SuccessResponse(
            "Caronas do motoristas recuperadas com sucesso",
            responseDtos
        );
    }

    public async Task<ApiResponse<string>> CancelRideAsync(Guid rideId, Guid driverId)
    {
        RideEntity? ride = await _dbContext.Rides
            .Include(r => r.Requests)
            .FirstOrDefaultAsync(r => r.Id == rideId);
            
        if (ride == null)
        {
            return ApiResponse<string>.FailureResponse("Carona não encontrada.", 404);
        }

        if (ride.DriverId != driverId)
        {
            return ApiResponse<string>.FailureResponse("Você não tem permissão para cancelar esta carona.", 403);
        }
        
        // Soft delete
        ride.Status = RideStatus.Canceled;

        // Cancel associated requests
        if (ride.Requests != null)
        {
            foreach (var request in ride.Requests)
            {
                if (request.Status == RideRequestStatus.Pending || request.Status == RideRequestStatus.Accepted)
                {
                    request.Status = RideRequestStatus.Cancelled;
                }
            }
        }

        await _dbContext.SaveChangesAsync();
        
        return ApiResponse<string>.SuccessResponse("Carona cancelada com sucesso.", null);
    }
}

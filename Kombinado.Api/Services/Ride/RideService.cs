using Kombinado.Api.Constants;
using Kombinado.Api.Data;
using Kombinado.Api.Extensions;
using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Models.DTOs.Responses;
using Kombinado.Api.Models.Entities;
using Kombinado.Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Kombinado.Api.Services.Ride;

public class RideService : IRideService
{
    // Same limit as the Origin/Destination columns (KombinadoDbContext)
    private const int MAX_LOCATION_LENGTH = 200;

    private readonly KombinadoDbContext _dbContext;
    public RideService(KombinadoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<RideResponseDto>> CreateRideAsync(CreateRideDto dto, Guid driverId)
    {
        // 1. Normalize and validate the input before touching the database
        dto.Origin = dto.Origin?.Trim() ?? string.Empty;
        dto.Destination = dto.Destination?.Trim() ?? string.Empty;
        DateTime departureTimeUtc = DateTimeUtils.ToUtc(dto.DepartureTime);

        string? validationError = ValidateRideData(dto, departureTimeUtc);
        if (validationError != null)
        {
            return ApiResponse<RideResponseDto>.FailureResponse(validationError, 400);
        }

        // 2. Check the offered seats against the driver's vehicle capacity
        var driver = await _dbContext.Users
            .Where(u => u.Id == driverId)
            .Select(u => new { u.Name, u.VehicleTotalSeats })
            .FirstOrDefaultAsync();

        if (driver == null)
        {
            return ApiResponse<RideResponseDto>.FailureResponse("Motorista não encontrado.", 404);
        }

        // Drivers registered before VehicleTotalSeats existed are only limited by MAX_TOTAL_SEATS
        if (driver.VehicleTotalSeats != null && dto.TotalSeats > driver.VehicleTotalSeats)
        {
            return ApiResponse<RideResponseDto>.FailureResponse(
                $"A carona não pode oferecer mais vagas do que o seu veículo possui ({driver.VehicleTotalSeats}).",
                400
            );
        }

        // 3. Block rides overlapping the driver's own rides or accepted seats
        DateTime? overlappingRide = await _dbContext.FindOverlappingOfferedRideAsync(driverId, departureTimeUtc);
        if (overlappingRide != null)
        {
            return ApiResponse<RideResponseDto>.FailureResponse(
                $"Você já oferece uma carona com saída em {DateTimeUtils.ToBrasiliaDisplay(overlappingRide.Value)}. Suas caronas precisam ter pelo menos 1 hora de diferença.",
                400
            );
        }

        DateTime? overlappingSeat = await _dbContext.FindOverlappingAcceptedSeatAsync(driverId, departureTimeUtc);
        if (overlappingSeat != null)
        {
            return ApiResponse<RideResponseDto>.FailureResponse(
                $"Você tem vaga aceita em uma carona com saída em {DateTimeUtils.ToBrasiliaDisplay(overlappingSeat.Value)}. Suas caronas precisam ter pelo menos 1 hora de diferença.",
                400
            );
        }

        // 4. Create a ride
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
        
        // 5. Save the ride in DB
        _dbContext.Rides.Add(newRide);
        await _dbContext.SaveChangesAsync();
        
        // 6. Return the Ride response
        RideResponseDto responseDto = new RideResponseDto
        {
            Id = newRide.Id,
            DriverName = driver.Name,
            Origin = newRide.Origin,
            Destination = newRide.Destination,
            DepartureTime = newRide.DepartureTime,
            AvailableSeats = newRide.AvailableSeats,
            TotalSeats = newRide.TotalSeats,
            Status = newRide.Status
        };
        
        return ApiResponse<RideResponseDto>.SuccessResponse("Carona criada com sucesso.", responseDto);
    }

    private static string? ValidateRideData(CreateRideDto dto, DateTime departureTimeUtc)
    {
        if (string.IsNullOrEmpty(dto.Origin) || string.IsNullOrEmpty(dto.Destination))
        {
            return "Informe a origem e o destino da carona.";
        }

        if (dto.Origin.Length > MAX_LOCATION_LENGTH || dto.Destination.Length > MAX_LOCATION_LENGTH)
        {
            return $"A origem e o destino devem ter no máximo {MAX_LOCATION_LENGTH} caracteres.";
        }

        if (string.Equals(dto.Origin, dto.Destination, StringComparison.OrdinalIgnoreCase))
        {
            return "A origem e o destino devem ser diferentes.";
        }

        if (departureTimeUtc <= DateTime.UtcNow)
        {
            return "O horário de partida deve ser uma data futura.";
        }

        if (dto.TotalSeats < VehicleUtils.MIN_TOTAL_SEATS || dto.TotalSeats > VehicleUtils.MAX_TOTAL_SEATS)
        {
            return $"A carona deve oferecer entre {VehicleUtils.MIN_TOTAL_SEATS} e {VehicleUtils.MAX_TOTAL_SEATS} vagas.";
        }

        return null;
    }

    public async Task<ApiResponse<IEnumerable<RideResponseDto>>> GetAvailableRidesAsync(Guid currentUserId)
    {
        await _dbContext.CompleteExpiredRidesAsync();

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
        await _dbContext.CompleteExpiredRidesAsync();

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

        if (ride.Status != RideStatus.Open && ride.Status != RideStatus.Full)
        {
            return ApiResponse<string>.FailureResponse("Apenas caronas abertas ou lotadas podem ser canceladas.", 400);
        }

        // Soft delete
        ride.Status = RideStatus.Canceled;

        // Accepted requests are kept for the trip history; the ride status tells it was canceled
        foreach (var request in ride.Requests.Where(r => r.Status == RideRequestStatus.Pending))
        {
            request.Status = RideRequestStatus.Cancelled;
        }

        await _dbContext.SaveChangesAsync();
        
        return ApiResponse<string>.SuccessResponse("Carona cancelada com sucesso.", null);
    }

    public async Task<ApiResponse<string>> CompleteRideAsync(Guid rideId, Guid driverId)
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
            return ApiResponse<string>.FailureResponse("Você não tem permissão para encerrar esta carona.", 403);
        }

        if (ride.Status != RideStatus.Open && ride.Status != RideStatus.Full)
        {
            return ApiResponse<string>.FailureResponse("Apenas caronas abertas ou lotadas podem ser encerradas.", 400);
        }

        if (ride.DepartureTime > DateTime.UtcNow)
        {
            return ApiResponse<string>.FailureResponse("A carona só pode ser encerrada depois do horário de saída.", 400);
        }

        ride.Status = RideStatus.Completed;

        // Accepted requests are kept for the trip history
        foreach (var request in ride.Requests.Where(r => r.Status == RideRequestStatus.Pending))
        {
            request.Status = RideRequestStatus.Cancelled;
        }

        await _dbContext.SaveChangesAsync();

        return ApiResponse<string>.SuccessResponse("Carona encerrada com sucesso.", null);
    }
}

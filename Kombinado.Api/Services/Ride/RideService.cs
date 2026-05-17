using Kombinado.Api.Constants;
using Kombinado.Api.Data;
using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Models.DTOs.Responses;
using Kombinado.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kombinado.Api.Services.Ride;

public class RideService : IRideService
{
    private readonly KombinadoDbContext _dbContext;

    public RideService(KombinadoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<RideResponseDto>> CreateRideAsync(CreateRideRequestDto requestDto, Guid driverId)
    {
        // 1. Create a ride
        RideEntity newRide = new RideEntity
        {
            Id = Guid.NewGuid(),
            DriverId = driverId,
            Origin = requestDto.Origin,
            Destination = requestDto.Destination,
            DepartureTime = requestDto.DepartureTime,
            AvailableSeats = requestDto.TotalSeats,
            TotalSeats = requestDto.TotalSeats,
            Status = RideStatus.Open
        };
        
        // 2. Save the ride in DB
        _dbContext.Rides.Add(newRide);
        await _dbContext.SaveChangesAsync();
        
        // 3. Return the Ride response
        RideResponseDto responseDto = new RideResponseDto
        {
            Id = newRide.Id,
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
            .Where(r => r.Status == RideStatus.Open && r.AvailableSeats > 0 && r.DriverId != currentUserId)
            .OrderBy(r => r.DepartureTime)
            .ToListAsync();
        
        List<RideResponseDto> responseDtos = availableRides.Select(r => new RideResponseDto
        {
            Id = r.Id,
            Origin = r.Origin,
            Destination = r.Destination,
            DepartureTime = r.DepartureTime,
            AvailableSeats = r.AvailableSeats,
            TotalSeats = r.TotalSeats,
            Status = r.Status
        }).ToList();
        
        return ApiResponse<IEnumerable<RideResponseDto>>.SuccessResponse(
            "Caronas disponíveis recuperadas com sucesso",
            responseDtos
        );
    }

    public async Task<ApiResponse<IEnumerable<RideResponseDto>>> GetMyDrivingRidesAsync(Guid driverId)
    {
        List<RideEntity> rides = await _dbContext.Rides
            .Where(r => r.DriverId == driverId)
            .ToListAsync();

        List<RideResponseDto> responseDtos = rides.Select(r => new RideResponseDto
        {
            Id = r.Id,
            Origin = r.Origin,
            Destination = r.Destination,
            DepartureTime = r.DepartureTime,
            AvailableSeats = r.AvailableSeats,
            TotalSeats = r.TotalSeats,
            Status = r.Status
        }).ToList();

        return ApiResponse<IEnumerable<RideResponseDto>>.SuccessResponse(
            "Caronas do motoristas recuperadas com sucesso",
            responseDtos
        );
    }
}
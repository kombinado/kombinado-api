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
}
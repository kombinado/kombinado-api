using Kombinado.Api.Data;
using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Models.DTOs.Responses;

namespace Kombinado.Api.Services.Ride;

public class RideService : IRideService
{
    private readonly KombinadoDbContext _dbContext;

    public RideService(KombinadoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ApiResponse<RideResponseDto>> CreateRideAsync(CreateRideRequestDto requestDto, Guid driverId)
    {
        throw new NotImplementedException();
    }
}
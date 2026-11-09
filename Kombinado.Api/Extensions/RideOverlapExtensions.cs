using Kombinado.Api.Constants;
using Kombinado.Api.Data;
using Kombinado.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kombinado.Api.Extensions;

// Rides overlap when their departure times are less than MIN_GAP apart; only Open/Full rides count
public static class RideOverlapExtensions
{
    private static readonly TimeSpan MIN_GAP = TimeSpan.FromHours(1);

    public static Task<DateTime?> FindOverlappingOfferedRideAsync(
        this KombinadoDbContext dbContext, Guid userId, DateTime departureTimeUtc)
    {
        DateTime from = departureTimeUtc - MIN_GAP;
        DateTime to = departureTimeUtc + MIN_GAP;

        return dbContext.Rides
            .Where(r => r.DriverId == userId &&
                        (r.Status == RideStatus.Open || r.Status == RideStatus.Full) &&
                        r.DepartureTime > from && r.DepartureTime < to)
            .OrderBy(r => r.DepartureTime)
            .Select(r => (DateTime?)r.DepartureTime)
            .FirstOrDefaultAsync();
    }

    public static Task<DateTime?> FindOverlappingAcceptedSeatAsync(
        this KombinadoDbContext dbContext, Guid userId, DateTime departureTimeUtc)
    {
        DateTime from = departureTimeUtc - MIN_GAP;
        DateTime to = departureTimeUtc + MIN_GAP;

        return dbContext.RideRequests
            .Where(rr => rr.PassengerId == userId &&
                         rr.Status == RideRequestStatus.Accepted &&
                         (rr.Ride.Status == RideStatus.Open || rr.Ride.Status == RideStatus.Full) &&
                         rr.Ride.DepartureTime > from && rr.Ride.DepartureTime < to)
            .OrderBy(rr => rr.Ride.DepartureTime)
            .Select(rr => (DateTime?)rr.Ride.DepartureTime)
            .FirstOrDefaultAsync();
    }

    // Tracked, so callers can change their status and save
    public static Task<List<RideRequestEntity>> GetOverlappingPendingRequestsAsync(
        this KombinadoDbContext dbContext, Guid userId, DateTime departureTimeUtc, Guid excludedRequestId)
    {
        DateTime from = departureTimeUtc - MIN_GAP;
        DateTime to = departureTimeUtc + MIN_GAP;

        return dbContext.RideRequests
            .Where(rr => rr.PassengerId == userId &&
                         rr.Id != excludedRequestId &&
                         rr.Status == RideRequestStatus.Pending &&
                         (rr.Ride.Status == RideStatus.Open || rr.Ride.Status == RideStatus.Full) &&
                         rr.Ride.DepartureTime > from && rr.Ride.DepartureTime < to)
            .ToListAsync();
    }
}

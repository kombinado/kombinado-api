using Kombinado.Api.Constants;
using Kombinado.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Kombinado.Api.Extensions;

public static class RideCompletionExtensions
{
    private static readonly TimeSpan AUTO_COMPLETE_DELAY = TimeSpan.FromHours(3);

    // Applied on reads instead of a background job, so the App Service doesn't need to stay always on
    public static async Task CompleteExpiredRidesAsync(this KombinadoDbContext dbContext)
    {
        DateTime cutoff = DateTime.UtcNow - AUTO_COMPLETE_DELAY;

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        // Must run before the rides update, which changes the status this filter relies on
        await dbContext.RideRequests
            .Where(rr => rr.Status == RideRequestStatus.Pending &&
                         (rr.Ride.Status == RideStatus.Open || rr.Ride.Status == RideStatus.Full) &&
                         rr.Ride.DepartureTime <= cutoff)
            .ExecuteUpdateAsync(s => s.SetProperty(rr => rr.Status, RideRequestStatus.Cancelled));

        await dbContext.Rides
            .Where(r => (r.Status == RideStatus.Open || r.Status == RideStatus.Full) &&
                        r.DepartureTime <= cutoff)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, RideStatus.Completed));

        await transaction.CommitAsync();
    }
}

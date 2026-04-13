using Kombinado.Api.Constants;

namespace Kombinado.Api.Models.Entities
{
    public class RideRequestEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Foreign keys and navigation properties
        public Guid RideId { get; set; }
        public RideEntity Ride { get; set; } = null!;

        // Passenger information
        public Guid PassengerId { get; set; }
        public UserEntity Passenger { get; set; } = null!;

        // Request status and metadata
        public string Status { get; set; } = RideRequestStatus.Pending;
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        // Optional meeting point suggestion from the passenger
        public string? MeetingPointSuggestion { get; set; }
    }
}

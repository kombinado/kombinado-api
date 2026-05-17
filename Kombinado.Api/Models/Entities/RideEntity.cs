using Kombinado.Api.Constants;

namespace Kombinado.Api.Models.Entities
{
    public class RideEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Foreign key to UserEntity (Driver)
        public Guid DriverId { get; set; }
        public UserEntity Driver { get; set; } = null!;

        // Ride details
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public string Status { get; set; } = RideStatus.Open;

        // Navigation property for ride requests
        public ICollection<RideRequestEntity> Requests { get; set; } = new List<RideRequestEntity>();
    }
}

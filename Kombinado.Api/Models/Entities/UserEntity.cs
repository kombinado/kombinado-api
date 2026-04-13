namespace Kombinado.Api.Models.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string WhatsApp { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public bool IsDriver { get; set; }

        // Vehicle data (only for drivers)
        public string? VehicleModel { get; set; }
        public string? VehicleColor { get; set; }
        public string? VehiclePlate { get; set; }

        // Navigation properties
        public ICollection<RideEntity> RidesOffered { get; set; } = new List<RideEntity>();
        public ICollection<RideRequestEntity> RequestsMade { get; set; } = new List<RideRequestEntity>();
    }
}

namespace Kombinado.Api.Models.DTOs.Requests
{
    public class SignupRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string WhatsApp { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public bool IsDriver { get; set; }

        // Vehicle data (only for drivers)
        public string? VehicleModel { get; set; }
        public string? VehicleColor { get; set; }
        public string? VehiclePlate { get; set; }
    }
}

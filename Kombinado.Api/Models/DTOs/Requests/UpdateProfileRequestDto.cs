namespace Kombinado.Api.Models.DTOs.Requests;

public class UpdateProfileRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;

    // Vehicle data (required for drivers, ignored for passengers)
    public string? VehicleModel { get; set; }
    public string? VehicleColor { get; set; }
    public string? VehiclePlate { get; set; }
    public int? VehicleTotalSeats { get; set; }
}

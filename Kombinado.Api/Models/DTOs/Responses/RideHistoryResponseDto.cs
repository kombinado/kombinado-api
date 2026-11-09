namespace Kombinado.Api.Models.DTOs.Responses;

public class RideHistoryResponseDto
{
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public int PassengersCount { get; set; }

    // Vehicle details
    public string? VehicleModel { get; set; }
    public string? VehicleColor { get; set; }
    public string? VehiclePlate { get; set; }
}

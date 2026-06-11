namespace Kombinado.Api.Models.DTOs.Responses;

public class MyRequestResponseDto
{
    public Guid Id { get; set; }
    public Guid RideId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? MeetingPointSuggestion { get; set; }
    public string? PhoneNumber { get; set; }
    
    // Ride details
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    
    // Vehicle details
    public string? VehicleModel { get; set; }
    public string? VehicleColor { get; set; }
    public string? VehiclePlate { get; set; }
}

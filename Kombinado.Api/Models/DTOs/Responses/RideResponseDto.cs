using Kombinado.Api.Constants;

namespace Kombinado.Api.Models.DTOs.Responses;

public class RideResponseDto
{
    public Guid Id { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public string Status { get; set; } = string.Empty;

    // Vehicle details
    public string? VehicleModel { get; set; }
    public string? VehicleColor { get; set; }
    public string? VehiclePlate { get; set; }
    
    // Auxiliary data
    public int PendingRequestsCount { get; set; }
}
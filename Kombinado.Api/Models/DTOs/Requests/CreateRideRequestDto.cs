namespace Kombinado.Api.Models.DTOs.Requests;

public class CreateRideRequestDto
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public int TotalSeats { get; set; }
}
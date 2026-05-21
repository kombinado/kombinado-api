namespace Kombinado.Api.Models.DTOs.Responses;

public class RideRequestResponseDto
{
    public Guid Id { get; set; }
    public string PassengerName { get; set; }
    public string Status { get; set; } 
    public string MeetingPointSuggestion { get; set; }
    public string? PhoneNumber { get; set; }
}
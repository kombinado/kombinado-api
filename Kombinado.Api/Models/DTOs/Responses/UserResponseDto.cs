namespace Kombinado.Api.Models.DTOs.Responses;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string WhatsApp { get; set; } = string.Empty;
    public bool IsDriver { get; set; }
    
    // Vehicle details (null if not driver or not provided)
    public string? VehicleModel { get; set; }
    public string? VehicleColor { get; set; }
    public string? VehiclePlate { get; set; }
}

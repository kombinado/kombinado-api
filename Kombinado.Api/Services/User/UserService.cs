using Kombinado.Api.Data;
using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Models.DTOs.Responses;
using Kombinado.Api.Models.Entities;
using Kombinado.Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Kombinado.Api.Services.User;

public class UserService : IUserService
{
    private readonly KombinadoDbContext _dbContext;
    public UserService(KombinadoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<UserResponseDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request)
    {
        // 1. Get the logged user
        UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return ApiResponse<UserResponseDto>.FailureResponse("Usuário não encontrado.", 404);
        }

        // 2. Normalize input
        request.Name = request.Name?.Trim() ?? string.Empty;
        request.Course = request.Course?.Trim() ?? string.Empty;
        request.WhatsApp = PhoneUtils.NormalizeWhatsApp(request.WhatsApp);
        request.VehicleModel = request.VehicleModel?.Trim();
        request.VehicleColor = request.VehicleColor?.Trim();
        request.VehiclePlate = VehicleUtils.NormalizePlate(request.VehiclePlate);

        // 3. Validate input (vehicle data only matters for drivers)
        string? validationError =
            UserValidationUtils.ValidatePersonalData(request.Name, request.WhatsApp, request.Course) ??
            (user.IsDriver
                ? UserValidationUtils.ValidateVehicleData(request.VehicleModel, request.VehicleColor, request.VehiclePlate, request.VehicleTotalSeats)
                : null);
        if (validationError != null)
        {
            return ApiResponse<UserResponseDto>.FailureResponse(validationError, 400);
        }

        // 4. Update the user (email and role are not editable)
        user.Name = request.Name;
        user.WhatsApp = request.WhatsApp;
        user.Course = request.Course;

        if (user.IsDriver)
        {
            user.VehicleModel = request.VehicleModel;
            user.VehicleColor = request.VehicleColor;
            user.VehiclePlate = request.VehiclePlate;
            user.VehicleTotalSeats = request.VehicleTotalSeats;
        }

        await _dbContext.SaveChangesAsync();

        // 5. Return the updated profile
        UserResponseDto responseDto = new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Course = user.Course,
            WhatsApp = user.WhatsApp,
            IsDriver = user.IsDriver,
            VehicleModel = user.VehicleModel,
            VehicleColor = user.VehicleColor,
            VehiclePlate = user.VehiclePlate,
            VehicleTotalSeats = user.VehicleTotalSeats
        };

        return ApiResponse<UserResponseDto>.SuccessResponse("Perfil atualizado com sucesso.", responseDto);
    }
}

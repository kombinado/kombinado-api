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
    private const int MAX_NAME_LENGTH = 100;
    private const int MAX_COURSE_LENGTH = 100;
    private const int MAX_VEHICLE_MODEL_LENGTH = 50;
    private const int MAX_VEHICLE_COLOR_LENGTH = 30;

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
        string? validationError = ValidatePersonalData(request) ?? (user.IsDriver ? ValidateVehicleData(request) : null);
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

    private static string? ValidatePersonalData(UpdateProfileRequestDto request)
    {
        if (string.IsNullOrEmpty(request.Name) || request.Name.Length > MAX_NAME_LENGTH)
        {
            return $"O nome é obrigatório e deve ter no máximo {MAX_NAME_LENGTH} caracteres.";
        }

        if (!PhoneUtils.IsValidWhatsApp(request.WhatsApp))
        {
            return "Informe um WhatsApp válido com DDD (10 ou 11 dígitos, sem o +55).";
        }

        if (string.IsNullOrEmpty(request.Course) || request.Course.Length > MAX_COURSE_LENGTH)
        {
            return $"O curso é obrigatório e deve ter no máximo {MAX_COURSE_LENGTH} caracteres.";
        }

        return null;
    }

    private static string? ValidateVehicleData(UpdateProfileRequestDto request)
    {
        if (string.IsNullOrEmpty(request.VehicleModel) ||
            string.IsNullOrEmpty(request.VehicleColor) ||
            string.IsNullOrEmpty(request.VehiclePlate))
        {
            return "Motoristas precisam informar o Modelo, Cor e Placa do veículo.";
        }

        if (request.VehicleModel.Length > MAX_VEHICLE_MODEL_LENGTH)
        {
            return $"O modelo do veículo deve ter no máximo {MAX_VEHICLE_MODEL_LENGTH} caracteres.";
        }

        if (request.VehicleColor.Length > MAX_VEHICLE_COLOR_LENGTH)
        {
            return $"A cor do veículo deve ter no máximo {MAX_VEHICLE_COLOR_LENGTH} caracteres.";
        }

        if (!VehicleUtils.IsValidPlate(request.VehiclePlate))
        {
            return "Placa inválida. Use o formato ABC1234 ou ABC1D23.";
        }

        if (!VehicleUtils.IsValidTotalSeats(request.VehicleTotalSeats))
        {
            return $"O número de vagas do veículo deve ser entre {VehicleUtils.MIN_TOTAL_SEATS} e {VehicleUtils.MAX_TOTAL_SEATS}.";
        }

        return null;
    }
}

using Kombinado.Api.Data;
using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Models.Entities;
using Kombinado.Api.Utils;
using Microsoft.EntityFrameworkCore;

namespace Kombinado.Api.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly KombinadoDbContext _dbContext;
        public AuthService(KombinadoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<string>> SignupAsync(SignupRequestDto request)
        {
            // 1. Check if email is student email
            if (!EmailUtils.IsStudentEmail(request.Email))
            {
                return ApiResponse<string>.FailureResponse("É necessário um e-mail institucional do IFTM (@estudante.iftm.edu.br).", 400);
            }

            // 2. If user is a driver, check if vehicle info is provided
            if (request.IsDriver)
            {
                if (string.IsNullOrWhiteSpace(request.VehicleModel) ||
                    string.IsNullOrWhiteSpace(request.VehicleColor) ||
                    string.IsNullOrWhiteSpace(request.VehiclePlate))
                {
                    return ApiResponse<string>.FailureResponse("Motoristas precisam informar o Modelo, Cor e Placa do veículo.", 400);
                }
            }

            // 3. Check if email is already registered
            if (await _dbContext.Users.AnyAsync(u => u.Email == request.Email))
            {
                return ApiResponse<string>.FailureResponse("Este e-mail já está cadastrado.", 400);
            }

            // 4. Trim password and validate strength
            request.Password = request.Password?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            {
                return ApiResponse<string>.FailureResponse("A senha deve ter no mínimo 8 caracteres.", 400);
            }

            if (!request.Password.Any(char.IsUpper) || !request.Password.Any(char.IsDigit))
            {
                return ApiResponse<string>.FailureResponse("A senha deve conter pelo menos uma letra maiúscula e um número.", 400);
            }

            // 5. Create new user
            UserEntity user = new UserEntity
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                WhatsApp = request.WhatsApp,
                Course = request.Course,
                IsDriver = request.IsDriver,
                VehicleModel = request.IsDriver ? request.VehicleModel : null,
                VehicleColor = request.IsDriver ? request.VehicleColor : null,
                VehiclePlate = request.IsDriver ? request.VehiclePlate : null
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            
            return ApiResponse<string>.SuccessResponse("Cadastro realizado com sucesso!");
        }
    }
}

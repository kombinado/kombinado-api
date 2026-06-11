using Kombinado.Api.Data;
using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Models.DTOs.Responses;
using Kombinado.Api.Models.Entities;
using Kombinado.Api.Services.Token;
using Kombinado.Api.Utils;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Kombinado.Api.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly KombinadoDbContext _dbContext;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthService(KombinadoDbContext dbContext, ITokenService tokenService, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _configuration = configuration;
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

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            // 1. Clear and normalize input (trim and lowercase email)
            request.Email = request.Email?.Trim().ToLower() ?? string.Empty;
            request.Password = request.Password?.Trim() ?? string.Empty;

            // 2. Get user by email (if exists)
            UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            // 3. Secure validation against enumeration (Check if user exists AND if password matches)
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return ApiResponse<LoginResponseDto>.FailureResponse("E-mail ou senha inválidos.", 401); 
            }

            // 4. Generate JWT token and refresh token 
            string accessToken = _tokenService.GenerateAccessToken(user);
            string refreshToken = _tokenService.GenerateRefreshToken();

            // 5. Save refresh token and expiry in database
            int refreshTokenExpiryDays = int.Parse(_configuration["JWT_REFRESH_EXPIRE_DAYS"] ?? "7");

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            // 6. Prepare response data
            LoginResponseDto responseData = new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Name = user.Name,
                IsDriver = user.IsDriver
            };

            return ApiResponse<LoginResponseDto>.SuccessResponse("Login realizado com sucesso!", responseData);
        }

        public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            // 1. Get claims from the expired access token
            ClaimsPrincipal principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);

            // Get the email claim
            string? email = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return ApiResponse<LoginResponseDto>.FailureResponse("Token inválido.", 400);
            }

            // 2. Get user by email (if exists)
            UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

            // 3. Maximum Security Validation (Rotation)
            // - Does the user exist?
            // - Does the scent Refresh Token match exactly with the one in the database?
            // - Is the Refresh Token still within the validity period (e.g., 7 days)?
            if (user == null ||
                user.RefreshToken != request.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return ApiResponse<LoginResponseDto>.FailureResponse("Sessão expirada. Por favor, faça login novamente.", 401);
            }

            // 4. Refresh Tokens (Rotation)
            string newAccessToken = _tokenService.GenerateAccessToken(user);
            string newRefreshToken = _tokenService.GenerateRefreshToken();

            // 5. Update user with new refresh token and expiry
            int refreshExpireDays = int.Parse(_configuration["JWT_REFRESH_EXPIRE_DAYS"] ?? "7");

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshExpireDays);

            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            // 6. Return the new tokens
            LoginResponseDto responseData = new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Name = user.Name,
                IsDriver = user.IsDriver
            };

            return ApiResponse<LoginResponseDto>.SuccessResponse("Tokens atualizados com sucesso!", responseData);
        }

        public async Task<ApiResponse<UserResponseDto>> GetUserProfileAsync(Guid userId)
        {
            UserResponseDto? user = await _dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    IsDriver = u.IsDriver,
                    VehicleModel = u.VehicleModel,
                    VehicleColor = u.VehicleColor,
                    VehiclePlate = u.VehiclePlate,
                    Course = u.Course,
                    WhatsApp = u.WhatsApp
                })
                .FirstOrDefaultAsync();
            if (user == null)
            {
                return ApiResponse<UserResponseDto>.FailureResponse("Usuário não encontrado.", 404);
            }

            return ApiResponse<UserResponseDto>.SuccessResponse("Perfil recuperado com sucesso.", user);
        }
    }
}

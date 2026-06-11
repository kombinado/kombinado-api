using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs;
using Kombinado.Api.Models.DTOs.Requests;
using Kombinado.Api.Models.DTOs.Responses;

namespace Kombinado.Api.Services.Auth
{
    public interface IAuthService
    {
        Task<ApiResponse<string>> SignupAsync(SignupRequestDto request);
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
        Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task<ApiResponse<UserResponseDto>> GetUserProfileAsync(Guid userId);
    }
}
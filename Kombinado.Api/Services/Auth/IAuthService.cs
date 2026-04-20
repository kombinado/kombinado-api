using Kombinado.Api.Models;
using Kombinado.Api.Models.DTOs.Requests;

namespace Kombinado.Api.Services.Auth
{
    public interface IAuthService
    {
        Task<ApiResponse<string>> SignupAsync(SignupRequestDto request);
    }
}
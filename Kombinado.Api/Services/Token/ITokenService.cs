using Kombinado.Api.Models.Entities;
using System.Security.Claims;

namespace Kombinado.Api.Services.Token
{
    public interface ITokenService
    {
        string GenerateAccessToken(UserEntity user);

        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Kombinado.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        string? userIdString = user.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
        {
            // If the user ID claim is missing or invalid, throw an exception or handle it as needed
            throw new UnauthorizedAccessException("Token inválido ou ID do usuário não encontrado.");
        }

        return userId;
    }
}
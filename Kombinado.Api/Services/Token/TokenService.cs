using Kombinado.Api.Constants;
using Kombinado.Api.Models.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Kombinado.Api.Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateAccessToken(UserEntity user)
        {
            // 1. Generate claims
            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtCustomClaims.IsDriver, user.IsDriver.ToString())
            };

            // 2. Get the secret key from .env 
            string? secretKey = _configuration["JWT_SECRET"] 
                ?? throw new InvalidOperationException("JWT_SECRET não configurado.");
            
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // 3. Create signing credentials
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            double expireMinutes = double.Parse(_configuration["JWT_EXPIRE_MINUTES"] ?? throw new InvalidOperationException("JWT_EXPIRE_MINUTES não configurado."));

            // 4. Build the token
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["JWT_ISSUER"],
                audience: _configuration["JWT_AUDIENCE"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: creds
            );

            // 5. Transform the token object into the Base64Url string that the frontend receives
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            byte[] randomNumber = new byte[64];

            // Use a secure random number generator to fill the byte array
            RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            string? secretKey = _configuration["JWT_SECRET"] 
                ?? throw new InvalidOperationException("JWT_SECRET não configurado.");

            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT_ISSUER"],
                ValidAudience = _configuration["JWT_AUDIENCE"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = false
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            // Check if the token is a JWT and if it uses the expected signing algorithm
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Token inválido");
            }

            return principal;
        }
    }
}

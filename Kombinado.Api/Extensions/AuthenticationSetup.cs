using System.Text;
using System.Text.Json;
using Kombinado.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Kombinado.Api.Extensions;

public static class AuthenticationSetup
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        string? secretKey = configuration["JWT_SECRET"] 
                            ?? throw new InvalidOperationException("JWT_SECRET não configurado.");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JWT_ISSUER"],
                    ValidAudience = configuration["JWT_AUDIENCE"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            
                options.Events = new JwtBearerEvents
                {
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                    
                        var response = ApiResponse<string>.FailureResponse("Acesso negado. Apenas motoristas podem acessar esta rota.", 403);
                        var json = JsonSerializer.Serialize(response);
                    
                        return context.Response.WriteAsync(json);
                    }
                };
            });

        return services;
    }
}
using Kombinado.Api.Constants;

namespace Kombinado.Api.Extensions;

public static class AuthorizationSetup
{
    public static IServiceCollection AddCustomPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // 1. Policy for drivers only
            options.AddPolicy("DriverOnly", policy => 
            {
                policy.RequireClaim(JwtCustomClaims.IsDriver, "True");
            });
        });

        return services;
    }
}
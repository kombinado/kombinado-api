using Kombinado.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kombinado.Api.Extensions;

public static class ControllersSetup
{
    private const string INVALID_REQUEST_MESSAGE = "Dados da requisição inválidos. Verifique se o corpo é um JSON válido e se os campos têm o tipo correto.";

    public static IServiceCollection AddControllersWithApiResponse(this IServiceCollection services)
    {
        services
            .AddControllers(options =>
            {
                // Empty or missing fields are validated by the services, with specific messages in Portuguese
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                // Malformed JSON, wrong types or invalid route values return the ApiResponse envelope instead of ProblemDetails
                options.InvalidModelStateResponseFactory = context =>
                {
                    // JSON errors are keyed by path (e.g. "$.totalSeats"), which tells the client which field to fix
                    string? field = context.ModelState
                        .Where(entry => entry.Key.StartsWith("$.") && entry.Value?.Errors.Count > 0)
                        .Select(entry => entry.Key[2..])
                        .FirstOrDefault();

                    string message = field == null ? INVALID_REQUEST_MESSAGE : $"{INVALID_REQUEST_MESSAGE} Campo: {field}.";

                    return new BadRequestObjectResult(ApiResponse<string>.FailureResponse(message, 400));
                };
            });

        return services;
    }
}

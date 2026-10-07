using System.IdentityModel.Tokens.Jwt;
using Kombinado.Api.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace Kombinado.Api.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        string method = httpContext.Request.Method;
        string path = httpContext.Request.Path;
        string traceId = httpContext.TraceIdentifier;
        string userId = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "anonymous";

        // 1. If the error is an UnauthorizedAccessException
        if (exception is UnauthorizedAccessException unauthorizedException)
        {
            _logger.LogWarning(
                "Unauthorized access on {Method} {Path} (TraceId: {TraceId}, UserId: {UserId}): {Message}",
                method, path, traceId, userId, unauthorizedException.Message
            );

            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

            var response = ApiResponse<string>.FailureResponse(unauthorizedException.Message, 401);
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true; // Returns true to indicate that the exception was handled
        }

        // 2. For any other unhandled exceptions, log it and return a generic 500 Internal Server Error response
        _logger.LogError(
            exception,
            "Unhandled exception on {Method} {Path} (TraceId: {TraceId}, UserId: {UserId})",
            method, path, traceId, userId
        );

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var serverError = ApiResponse<string>.FailureResponse("Ocorreu um erro interno no servidor. Tente novamente mais tarde.", 500);
        await httpContext.Response.WriteAsJsonAsync(serverError, cancellationToken);

        return true;
    }
}

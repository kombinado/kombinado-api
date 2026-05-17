using Kombinado.Api.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace Kombinado.Api.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // 1. If the error is an UnauthorizedAccessException
        if (exception is UnauthorizedAccessException unauthorizedException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            
            var response = ApiResponse<string>.FailureResponse(unauthorizedException.Message, 401);
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            
            return true; // Returns true to indicate that the exception was handled
        }

        // 2. For any other unhandled exceptions, return a generic 500 Internal Server Error response
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        
        var serverError = ApiResponse<string>.FailureResponse("Ocorreu um erro interno no servidor. Tente novamente mais tarde.", 500);
        await httpContext.Response.WriteAsJsonAsync(serverError, cancellationToken);

        return true;
    }
}
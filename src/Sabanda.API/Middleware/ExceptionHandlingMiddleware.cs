using Sabanda.Application.Common.Exceptions;
using ValidationException = Sabanda.Application.Common.Exceptions.ValidationException;

namespace Sabanda.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        context.Response.ContentType = "application/problem+json";

        var (status, title, type, errors) = exception switch
        {
            NotFoundException e    => (404, "Not Found",           "https://sabanda.app/errors/not-found",          (IDictionary<string, string[]>?)null),
            ForbiddenException e   => (403, "Forbidden",           "https://sabanda.app/errors/forbidden",          null),
            UnauthorizedException e=> (401, "Unauthorized",        "https://sabanda.app/errors/unauthorized",       null),
            ConflictException e    => (409, "Conflict",            "https://sabanda.app/errors/conflict",           null),
            ValidationException e  => (422, "Validation Error",    "https://sabanda.app/errors/validation",         e.Errors),
            TenantResolutionException e => (400, "Bad Request",    "https://sabanda.app/errors/tenant-required",    null),
            _                      => (500, "Internal Server Error","https://sabanda.app/errors/server-error",      null),
        };

        context.Response.StatusCode = status;

        object response = errors != null
            ? new { type, title, status, detail = exception.Message, errors }
            : new { type, title, status, detail = status == 500 ? "An unexpected error occurred." : exception.Message };

        await context.Response.WriteAsJsonAsync(response);
    }
}

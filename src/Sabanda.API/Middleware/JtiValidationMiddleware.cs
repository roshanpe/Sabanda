using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Sabanda.Infrastructure.Persistence;

namespace Sabanda.API.Middleware;

public class JtiValidationMiddleware
{
    private readonly RequestDelegate _next;

    public JtiValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, SabandaDbContext db)
    {
        // Only validate authenticated requests
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var jti = context.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (string.IsNullOrEmpty(jti))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://sabanda.app/errors/unauthorized",
                title = "Unauthorized",
                status = 401,
                detail = "Token is missing required claims."
            });
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var isValid = await db.ActiveSessions
            .AsNoTracking()
            .AnyAsync(s => s.Jti == jti && s.RevokedAt == null && s.ExpiresAt > now);

        if (!isValid)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://sabanda.app/errors/unauthorized",
                title = "Unauthorized",
                status = 401,
                detail = "Session has expired or been revoked."
            });
            return;
        }

        await _next(context);
    }
}

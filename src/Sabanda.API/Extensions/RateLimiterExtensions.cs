using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using Sabanda.API.Settings;

namespace Sabanda.API.Extensions;

public static class RateLimiterExtensions
{
    public static IServiceCollection AddSabandaRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Login: fixed window per IP — read settings at partition-creation time so test overrides take effect
            options.AddPolicy("loginPolicy", httpContext =>
            {
                var settings = httpContext.RequestServices
                    .GetRequiredService<IOptions<RateLimiterSettings>>().Value;
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.LoginPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    });
            });

            // Authenticated: sliding window per user
            options.AddPolicy("authenticatedPolicy", httpContext =>
            {
                var settings = httpContext.RequestServices
                    .GetRequiredService<IOptions<RateLimiterSettings>>().Value;
                var userId = httpContext.User.Identity?.IsAuthenticated == true
                    ? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    : null;
                var key = userId ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: key,
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = settings.AuthenticatedPerMinute,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    });
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    type = "https://sabanda.app/errors/rate-limit",
                    title = "Too Many Requests",
                    status = 429,
                    detail = "Rate limit exceeded. Please try again later."
                }, token);
            };
        });

        return services;
    }
}

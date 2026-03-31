using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Infrastructure.Persistence;

namespace Sabanda.API.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenantService tenantService, SabandaDbContext db)
    {
        // First try X-Tenant-Slug header
        if (context.Request.Headers.TryGetValue("X-Tenant-Slug", out var slugHeader) && !string.IsNullOrWhiteSpace(slugHeader))
        {
            var slug = slugHeader.ToString().ToLowerInvariant();
            var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug && t.IsActive);
            if (tenant != null)
            {
                tenantService.SetTenant(tenant.Id, tenant.Slug);
                await _next(context);
                return;
            }
        }

        // Then try tenant_id claim from bearer token (if present)
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var tokenString = authHeader["Bearer ".Length..].Trim();
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(tokenString))
                {
                    var jwtToken = handler.ReadJwtToken(tokenString);
                    var tenantIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
                    if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim, out var tenantId))
                    {
                        var tenant = await db.Tenants.AsNoTracking()
                            .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);
                        if (tenant != null)
                        {
                            tenantService.SetTenant(tenant.Id, tenant.Slug);
                            await _next(context);
                            return;
                        }
                    }
                }
            }
            catch
            {
                // Invalid token format — fall through to 400 below
            }
        }

        // Unauthenticated public paths that don't need a tenant (Swagger, health)
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/swagger") || path.StartsWith("/health") || path.StartsWith("/hangfire"))
        {
            await _next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://sabanda.app/errors/tenant-required",
            title = "Tenant Required",
            status = 400,
            detail = "Provide the X-Tenant-Slug header or a valid Bearer token with a tenant_id claim."
        });
    }
}

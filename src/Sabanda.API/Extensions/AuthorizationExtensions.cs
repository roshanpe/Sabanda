using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Sabanda.API.Settings;

namespace Sabanda.API.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
            });

        // Defer reading JWT settings so test config overrides are applied before validation
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((bearerOptions, jwtSettings) =>
            {
                var s = jwtSettings.Value;
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = s.Issuer,
                    ValidAudience = s.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.SigningKey)),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = "role",
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin", p => p.RequireRole("Administrator"));
            options.AddPolicy("RequireStaff", p =>
                p.RequireRole("Administrator", "PrimaryAccountHolder", "ProgramCoordinator", "EventCoordinator"));
            options.AddPolicy("RequireCoordinator", p =>
                p.RequireRole("Administrator", "ProgramCoordinator", "EventCoordinator"));
            options.AddPolicy("RequireAuthenticated", p => p.RequireAuthenticatedUser());
        });

        return services;
    }
}

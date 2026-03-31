using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;

namespace Sabanda.Infrastructure.Services;

public class QrTokenService : IQrTokenService
{
    private readonly IConfiguration _configuration;

    public QrTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<QrTokenResult> IssueAsync(Guid subjectId, string type, Guid tenantId)
    {
        var expiryDays = int.Parse(_configuration["QrToken:ExpiryDays"] ?? "90");
        var signingKey = _configuration["QrToken:SigningKey"]!;
        var now = DateTimeOffset.UtcNow;
        var jti = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subjectId.ToString()),
            new("type", type),
            new("tenant_id", tenantId.ToString()),
            new(JwtRegisteredClaimNames.Jti, jti.ToString()),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "sabanda-qr",
            audience: "sabanda-qr",
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.AddDays(expiryDays).UtcDateTime,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(new QrTokenResult(tokenString, jti));
    }

    public QrLookupResult ValidateAndLookup(string token)
    {
        var signingKey = _configuration["QrToken:SigningKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

        var validationParams = new TokenValidationParameters
        {
            ValidIssuer = "sabanda-qr",
            ValidAudience = "sabanda-qr",
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };

        try
        {
            var handler = new JwtSecurityTokenHandler();
            handler.InboundClaimTypeMap.Clear();
            var principal = handler.ValidateToken(token, validationParams, out _);

            var subjectId = Guid.Parse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var type = principal.FindFirstValue("type")!;
            var tenantId = Guid.Parse(principal.FindFirstValue("tenant_id")!);
            var jti = Guid.Parse(principal.FindFirstValue(JwtRegisteredClaimNames.Jti)!);

            return new QrLookupResult(type, subjectId, tenantId, jti);
        }
        catch (SecurityTokenException)
        {
            throw new UnauthorizedException("QR token is invalid or expired.");
        }
    }
}

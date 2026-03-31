using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Entities;
using Sabanda.Infrastructure.Persistence;

namespace Sabanda.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly SabandaDbContext _db;
    private readonly IConfiguration _configuration;

    public TokenService(SabandaDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<string> IssueTokenAsync(AppUser user)
    {
        var expiryHours = int.Parse(_configuration["Jwt:ExpiryHours"] ?? "8");
        var signingKey = _configuration["Jwt:SigningKey"]!;
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddHours(expiryHours);
        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("role", user.Role.ToString()),
            new("tenant_id", user.TenantId.ToString()),
            new("family_id", user.FamilyId?.ToString() ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "sabanda",
            audience: _configuration["Jwt:Audience"] ?? "sabanda",
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        // Track this session
        var session = new ActiveSession(user.Id, jti, now, expiresAt);
        await _db.ActiveSessions.AddAsync(session);

        return tokenString;
    }

    public async Task RevokeAsync(string jti)
    {
        var session = await _db.ActiveSessions
            .FirstOrDefaultAsync(s => s.Jti == jti && s.RevokedAt == null);
        if (session != null)
        {
            session.Revoke();
            await _db.SaveChangesAsync();
        }
    }

    public async Task RevokeAllForUserAsync(Guid userId)
    {
        var sessions = await _db.ActiveSessions
            .Where(s => s.UserId == userId && s.RevokedAt == null)
            .ToListAsync();
        foreach (var session in sessions)
            session.Revoke();
        await _db.SaveChangesAsync();
    }
}

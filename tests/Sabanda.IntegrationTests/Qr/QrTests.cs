using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sabanda.Application.Auth.DTOs;
using Sabanda.Application.Families.DTOs;
using Sabanda.Application.Members.DTOs;
using Sabanda.Application.Qr.Queries;
using Sabanda.IntegrationTests.Fixtures;
using Xunit;

namespace Sabanda.IntegrationTests.Qr;

[Collection("Database")]
public class QrTests
{
    private readonly DatabaseFixture _fixture;
    private readonly HttpClient _adminClient;

    public QrTests(DatabaseFixture fixture)
    {
        _fixture = fixture;

        var loginClient = fixture.Factory.CreateClient();
        loginClient.DefaultRequestHeaders.Add("X-Tenant-Slug", fixture.TenantSlug);
        var loginResp = loginClient.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = fixture.AdminEmail, Password = fixture.AdminPassword }).GetAwaiter().GetResult();
        var loginBody = loginResp.Content.ReadFromJsonAsync<LoginResponse>().GetAwaiter().GetResult();

        _adminClient = fixture.Factory.CreateClient();
        _adminClient.DefaultRequestHeaders.Add("X-Tenant-Slug", fixture.TenantSlug);
        _adminClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody!.Token);
    }

    private async Task<FamilyResponse> CreateTestFamily(string suffix = "")
    {
        var resp = await _adminClient.PostAsJsonAsync("/api/v1/families",
            new CreateFamilyRequest($"QR Family {suffix}", $"qr_{suffix}_{Guid.NewGuid():N}@test.com", "Password@123!"));
        return (await resp.Content.ReadFromJsonAsync<FamilyResponse>())!;
    }

    [Fact]
    public async Task RegenerateFamily_Returns200WithToken()
    {
        var family = await CreateTestFamily("regen");

        var response = await _adminClient.PostAsync($"/api/v1/families/{family.Id}/qr/regenerate", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        body!["token"].Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task QrLookup_WithValidToken_Returns200()
    {
        var family = await CreateTestFamily("lookup");
        var regenResp = await _adminClient.PostAsync($"/api/v1/families/{family.Id}/qr/regenerate", null);
        var tokenBody = await regenResp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        var token = tokenBody!["token"];

        var response = await _adminClient.GetAsync($"/api/v1/qr/lookup?token={Uri.EscapeDataString(token)}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<QrLookupResponse>();
        body!.SubjectType.Should().Be("family");
        body.Family!.Id.Should().Be(family.Id);
    }

    [Fact]
    public async Task QrLookup_WithoutAuthJwt_Returns401()
    {
        var family = await CreateTestFamily("noauth");
        var regenResp = await _adminClient.PostAsync($"/api/v1/families/{family.Id}/qr/regenerate", null);
        var tokenBody = await regenResp.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        var token = tokenBody!["token"];

        var unauthClient = _fixture.Factory.CreateClient();
        unauthClient.DefaultRequestHeaders.Add("X-Tenant-Slug", _fixture.TenantSlug);

        var response = await unauthClient.GetAsync($"/api/v1/qr/lookup?token={Uri.EscapeDataString(token)}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task QrLookup_ExpiredToken_Returns401()
    {
        var family = await CreateTestFamily("expired");

        // Build an expired QR token manually
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-qr-token-signing-key-at-least-32chars"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var past = DateTime.UtcNow.AddSeconds(-1);
        var expiredToken = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: "sabanda-qr",
            audience: "sabanda-qr",
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, family.Id.ToString()),
                new Claim("type", "family"),
                new Claim("tenant_id", _fixture.TenantId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            },
            notBefore: past.AddSeconds(-10),
            expires: past,
            signingCredentials: creds));

        var response = await _adminClient.GetAsync($"/api/v1/qr/lookup?token={Uri.EscapeDataString(expiredToken)}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task QrLookup_AfterRegenerate_OldTokenReturns404()
    {
        var family = await CreateTestFamily("rotate");

        // Get first token
        var resp1 = await _adminClient.PostAsync($"/api/v1/families/{family.Id}/qr/regenerate", null);
        var oldToken = (await resp1.Content.ReadFromJsonAsync<Dictionary<string, string>>())!["token"];

        // Regenerate again — invalidates old token
        await _adminClient.PostAsync($"/api/v1/families/{family.Id}/qr/regenerate", null);

        // Old token lookup should return 404 (jti no longer matches current QrTokenJti)
        var response = await _adminClient.GetAsync($"/api/v1/qr/lookup?token={Uri.EscapeDataString(oldToken)}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task QrLookup_Minor_WritesAuditLog()
    {
        var family = await CreateTestFamily("minor_audit");

        var memberRequest = new CreateMemberRequest(
            FullName: "QR Minor",
            DateOfBirth: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5)),
            ConsentGiven: true,
            ConsentGivenBy: Guid.NewGuid(),
            ConsentGivenAt: DateTimeOffset.UtcNow);
        var memberResp = await _adminClient.PostAsJsonAsync(
            $"/api/v1/families/{family.Id}/members", memberRequest);
        var member = (await memberResp.Content.ReadFromJsonAsync<MemberResponse>())!;

        var regenResp = await _adminClient.PostAsync($"/api/v1/members/{member.Id}/qr/regenerate", null);
        var token = (await regenResp.Content.ReadFromJsonAsync<Dictionary<string, string>>())!["token"];

        var before = DateTimeOffset.UtcNow;
        await _adminClient.GetAsync($"/api/v1/qr/lookup?token={Uri.EscapeDataString(token)}");

        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Sabanda.Infrastructure.Persistence.SabandaDbContext>();
        var count = db.AuditLogs.Count(a =>
            a.TargetEntityId == member.Id
            && a.Action == Sabanda.Domain.Enums.AuditAction.MinorDataRead
            && a.CreatedAt >= before);

        count.Should().Be(1);
    }
}

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Sabanda.Application.Auth.DTOs;
using Sabanda.IntegrationTests.Fixtures;
using Xunit;

namespace Sabanda.IntegrationTests.Auth;

[Collection("Database")]
public class LoginTests
{
    private readonly DatabaseFixture _fixture;
    private readonly HttpClient _client;

    public LoginTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.Factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-Slug", fixture.TenantSlug);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithToken()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = _fixture.AdminEmail, Password = _fixture.AdminPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
        body.Role.Should().Be(Sabanda.Domain.Enums.UserRole.Administrator);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = _fixture.AdminEmail, Password = "WrongPassword!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_FiveFailures_LocksAccount()
    {
        // Use a unique email so we don't affect other tests
        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Sabanda.Infrastructure.Persistence.SabandaDbContext>();
        var hash = BCrypt.Net.BCrypt.HashPassword("correct", workFactor: 4);
        var user = new Sabanda.Domain.Entities.AppUser(_fixture.TenantId, "locktest@test.com", hash, Sabanda.Domain.Enums.UserRole.FamilyMember);
        await db.AppUsers.AddAsync(user);
        await db.SaveChangesAsync();

        for (int i = 0; i < 5; i++)
        {
            await _client.PostAsJsonAsync("/api/v1/auth/login",
                new { Email = "locktest@test.com", Password = "wrong" });
        }

        // Even with correct password, account is locked
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = "locktest@test.com", Password = "correct" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_MissingTenantHeader_Returns400()
    {
        var clientWithoutTenant = _fixture.Factory.CreateClient();
        var response = await clientWithoutTenant.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = _fixture.AdminEmail, Password = _fixture.AdminPassword });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AuthenticatedEndpoint_WithValidToken_Succeeds()
    {
        // Login to get token
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = _fixture.AdminEmail, Password = _fixture.AdminPassword });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        // Use token on logout endpoint
        var authClient = _fixture.Factory.CreateClient();
        authClient.DefaultRequestHeaders.Add("X-Tenant-Slug", _fixture.TenantSlug);
        authClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody!.Token);

        var logoutResponse = await authClient.PostAsync("/api/v1/auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AuthenticatedEndpoint_AfterLogout_Returns401()
    {
        // Login
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = _fixture.AdminEmail, Password = _fixture.AdminPassword });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var token = loginBody!.Token;

        var authClient = _fixture.Factory.CreateClient();
        authClient.DefaultRequestHeaders.Add("X-Tenant-Slug", _fixture.TenantSlug);
        authClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Logout
        await authClient.PostAsync("/api/v1/auth/logout", null);

        // Same token should now be rejected
        var secondLogout = await authClient.PostAsync("/api/v1/auth/logout", null);
        secondLogout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TenantIsolation_TenantACannotSeeTenantBData()
    {
        // Create a second tenant
        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Sabanda.Infrastructure.Persistence.SabandaDbContext>();
        var tenantB = new Sabanda.Domain.Entities.Tenant("Tenant B", "tenant-b");
        await db.Tenants.AddAsync(tenantB);
        await db.SaveChangesAsync();

        // Add a family to Tenant B (bypass tenant filter)
        var familyB = new Sabanda.Domain.Entities.Family(tenantB.Id, "Family B", Guid.NewGuid());
        await db.Families.AddAsync(familyB);
        await db.SaveChangesAsync();

        // Querying as Tenant A should not return Tenant B's family
        var clientA = _fixture.Factory.CreateClient();
        clientA.DefaultRequestHeaders.Add("X-Tenant-Slug", _fixture.TenantSlug);
        var loginResp = await clientA.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = _fixture.AdminEmail, Password = _fixture.AdminPassword });
        var token = (await loginResp.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
        clientA.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resp = await clientA.GetAsync($"/api/v1/families/{familyB.Id}");
        // Should be 404 — Tenant A cannot see Tenant B's family
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

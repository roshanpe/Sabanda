using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Sabanda.Application.Auth.DTOs;
using Sabanda.Application.Families.DTOs;
using Sabanda.Application.Members.DTOs;
using Sabanda.Domain.Entities;
using Sabanda.Infrastructure.Persistence;
using Sabanda.IntegrationTests.Fixtures;
using Xunit;

namespace Sabanda.IntegrationTests.Families;

[Collection("Database")]
public class FamilyTests
{
    private readonly DatabaseFixture _fixture;
    private readonly HttpClient _adminClient;

    public FamilyTests(DatabaseFixture fixture)
    {
        _fixture = fixture;

        // Create and log in admin client once per test class run
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

    [Fact]
    public async Task CreateFamily_AsAdmin_Returns201WithFamily()
    {
        var request = new CreateFamilyRequest(
            "Smith Family",
            $"smith_{Guid.NewGuid():N}@test.com",
            "Password@123!");

        var response = await _adminClient.PostAsJsonAsync("/api/v1/families", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<FamilyResponse>();
        body!.DisplayName.Should().Be("Smith Family");
        body.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateFamily_FamilyAndUserCreatedAtomically()
    {
        var email = $"atomic_{Guid.NewGuid():N}@test.com";
        var request = new CreateFamilyRequest("Jones Family", email, "Password@123!");

        var response = await _adminClient.PostAsJsonAsync("/api/v1/families", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var family = await response.Content.ReadFromJsonAsync<FamilyResponse>();

        // Verify both family and user exist in DB
        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SabandaDbContext>();
        var user = db.AppUsers.FirstOrDefault(u => u.Email == email);
        user.Should().NotBeNull();
        user!.FamilyId.Should().Be(family!.Id);
    }

    [Fact]
    public async Task CreateFamily_DuplicateEmail_Returns409()
    {
        var email = $"dup_{Guid.NewGuid():N}@test.com";
        var request = new CreateFamilyRequest("Family A", email, "Password@123!");
        await _adminClient.PostAsJsonAsync("/api/v1/families", request);

        var response = await _adminClient.PostAsJsonAsync("/api/v1/families",
            new CreateFamilyRequest("Family B", email, "Password@123!"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetFamily_AsAdmin_Returns200()
    {
        var createResp = await _adminClient.PostAsJsonAsync("/api/v1/families",
            new CreateFamilyRequest("Brown Family", $"brown_{Guid.NewGuid():N}@test.com", "Password@123!"));
        var family = await createResp.Content.ReadFromJsonAsync<FamilyResponse>();

        var response = await _adminClient.GetAsync($"/api/v1/families/{family!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<FamilyResponse>();
        body!.Id.Should().Be(family.Id);
    }

    [Fact]
    public async Task CreateMember_Adult_Returns201()
    {
        var createFamilyResp = await _adminClient.PostAsJsonAsync("/api/v1/families",
            new CreateFamilyRequest("Adult Family", $"adult_{Guid.NewGuid():N}@test.com", "Password@123!"));
        var family = await createFamilyResp.Content.ReadFromJsonAsync<FamilyResponse>();

        var memberRequest = new CreateMemberRequest(
            FullName: "John Adult",
            DateOfBirth: new DateOnly(1990, 1, 1));

        var response = await _adminClient.PostAsJsonAsync(
            $"/api/v1/families/{family!.Id}/members", memberRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var member = await response.Content.ReadFromJsonAsync<MemberResponse>();
        member!.FullName.Should().Be("John Adult");
        member.IsAdult.Should().BeTrue();
    }

    [Fact]
    public async Task CreateMember_MinorWithoutConsent_Returns422()
    {
        var createFamilyResp = await _adminClient.PostAsJsonAsync("/api/v1/families",
            new CreateFamilyRequest("Minor Family", $"minor_{Guid.NewGuid():N}@test.com", "Password@123!"));
        var family = await createFamilyResp.Content.ReadFromJsonAsync<FamilyResponse>();

        // Minor (born 5 years ago) with no consent fields
        var memberRequest = new CreateMemberRequest(
            FullName: "Young Minor",
            DateOfBirth: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5)));

        var response = await _adminClient.PostAsJsonAsync(
            $"/api/v1/families/{family!.Id}/members", memberRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreateMember_MinorWithConsent_Returns201()
    {
        var createFamilyResp = await _adminClient.PostAsJsonAsync("/api/v1/families",
            new CreateFamilyRequest("Consent Family", $"consent_{Guid.NewGuid():N}@test.com", "Password@123!"));
        var family = await createFamilyResp.Content.ReadFromJsonAsync<FamilyResponse>();

        var memberRequest = new CreateMemberRequest(
            FullName: "Young Minor",
            DateOfBirth: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5)),
            ConsentGiven: true,
            ConsentGivenBy: Guid.NewGuid(),
            ConsentGivenAt: DateTimeOffset.UtcNow);

        var response = await _adminClient.PostAsJsonAsync(
            $"/api/v1/families/{family!.Id}/members", memberRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var member = await response.Content.ReadFromJsonAsync<MemberResponse>();
        member!.ConsentGiven.Should().BeTrue();
    }

    [Fact]
    public async Task GetMember_Minor_LogsMinorDataRead()
    {
        var createFamilyResp = await _adminClient.PostAsJsonAsync("/api/v1/families",
            new CreateFamilyRequest("Audit Family", $"audit_{Guid.NewGuid():N}@test.com", "Password@123!"));
        var family = await createFamilyResp.Content.ReadFromJsonAsync<FamilyResponse>();

        var memberRequest = new CreateMemberRequest(
            FullName: "Audit Minor",
            DateOfBirth: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5)),
            ConsentGiven: true,
            ConsentGivenBy: Guid.NewGuid(),
            ConsentGivenAt: DateTimeOffset.UtcNow);
        var createMemberResp = await _adminClient.PostAsJsonAsync(
            $"/api/v1/families/{family!.Id}/members", memberRequest);
        var member = await createMemberResp.Content.ReadFromJsonAsync<MemberResponse>();

        // Read the member (should trigger MinorDataRead audit log)
        var before = DateTimeOffset.UtcNow;
        await _adminClient.GetAsync($"/api/v1/members/{member!.Id}");
        await _adminClient.GetAsync($"/api/v1/members/{member.Id}");

        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SabandaDbContext>();
        var auditCount = db.AuditLogs
            .Count(a => a.TargetEntityId == member.Id
                && a.Action == Sabanda.Domain.Enums.AuditAction.MinorDataRead
                && a.CreatedAt >= before);

        auditCount.Should().Be(2, "each GET of a minor should produce exactly one MinorDataRead log");
    }
}

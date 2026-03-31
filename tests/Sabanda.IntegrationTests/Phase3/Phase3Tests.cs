using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Sabanda.Application.Auth.DTOs;
using Sabanda.Application.Events.DTOs;
using Sabanda.Application.Families.DTOs;
using Sabanda.Application.Memberships.DTOs;
using Sabanda.Application.Programs.DTOs;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;
using Sabanda.Infrastructure.Persistence;
using Sabanda.IntegrationTests.Fixtures;
using Xunit;

namespace Sabanda.IntegrationTests.Phase3;

[Collection("Database")]
public class Phase3Tests
{
    private readonly DatabaseFixture _fixture;
    private readonly HttpClient _adminClient;

    public Phase3Tests(DatabaseFixture fixture)
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

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<FamilyResponse> CreateFamilyAsync(string prefix = "p3")
    {
        var resp = await _adminClient.PostAsJsonAsync("/api/v1/families",
            new CreateFamilyRequest($"{prefix} Family", $"{prefix}_{Guid.NewGuid():N}@test.com", "Password@123!"));
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<FamilyResponse>())!;
    }

    private async Task<MembershipResponse> CreateCompletedMembershipAsync(Guid familyId, MembershipType type)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var createResp = await _adminClient.PostAsJsonAsync("/api/v1/memberships",
            new CreateMembershipRequest(familyId, type, today, today.AddYears(1)));
        createResp.EnsureSuccessStatusCode();
        var membership = (await createResp.Content.ReadFromJsonAsync<MembershipResponse>())!;

        // Initiated → Pending → Completed
        await _adminClient.PatchAsJsonAsync($"/api/v1/memberships/{membership.Id}/payment-status",
            new UpdatePaymentStatusRequest(PaymentStatus.Pending));
        var completedResp = await _adminClient.PatchAsJsonAsync($"/api/v1/memberships/{membership.Id}/payment-status",
            new UpdatePaymentStatusRequest(PaymentStatus.Completed));
        completedResp.EnsureSuccessStatusCode();
        return (await completedResp.Content.ReadFromJsonAsync<MembershipResponse>())!;
    }

    // ── Membership tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateMembership_OverlappingDates_Returns409()
    {
        var family = await CreateFamilyAsync("overlap");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // First membership
        var r1 = await _adminClient.PostAsJsonAsync("/api/v1/memberships",
            new CreateMembershipRequest(family.Id, MembershipType.Program, today, today.AddYears(1)));
        r1.StatusCode.Should().Be(HttpStatusCode.Created);

        // Overlapping second membership (same type, same family)
        var r2 = await _adminClient.PostAsJsonAsync("/api/v1/memberships",
            new CreateMembershipRequest(family.Id, MembershipType.Program, today.AddMonths(6), today.AddYears(2)));

        r2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdatePaymentStatus_InvalidTransition_Returns422()
    {
        var family = await CreateFamilyAsync("badtransition");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var createResp = await _adminClient.PostAsJsonAsync("/api/v1/memberships",
            new CreateMembershipRequest(family.Id, MembershipType.Event, today, today.AddYears(1)));
        var membership = (await createResp.Content.ReadFromJsonAsync<MembershipResponse>())!;

        // Initiated → Completed (skipping Pending) is invalid
        var resp = await _adminClient.PatchAsJsonAsync($"/api/v1/memberships/{membership.Id}/payment-status",
            new UpdatePaymentStatusRequest(PaymentStatus.Completed));

        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── Program enrolment tests ────────────────────────────────────────────────

    [Fact]
    public async Task EnrolMember_WithoutActiveMembership_Returns422()
    {
        var family = await CreateFamilyAsync("nomembership");
        // Create a member (adult)
        var memberResp = await _adminClient.PostAsJsonAsync($"/api/v1/families/{family.Id}/members",
            new { FullName = "Test Member", DateOfBirth = new DateOnly(1990, 1, 1) });
        var member = (await memberResp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>())!;
        var memberId = member.GetProperty("id").GetGuid();

        // Create a program
        var programResp = await _adminClient.PostAsJsonAsync("/api/v1/programs",
            new CreateProgramRequest("Test Program", 10));
        programResp.EnsureSuccessStatusCode();
        var program = (await programResp.Content.ReadFromJsonAsync<ProgramResponse>())!;

        // Enrol without active membership
        var enrolResp = await _adminClient.PostAsJsonAsync($"/api/v1/programs/{program.Id}/enrolments",
            new EnrolMemberRequest(memberId));

        enrolResp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task EnrolMember_WhenAtCapacity_Returns201WithWaitlistedStatus()
    {
        // Program with capacity 1
        var programResp = await _adminClient.PostAsJsonAsync("/api/v1/programs",
            new CreateProgramRequest("Capacity1 Program", 1));
        programResp.EnsureSuccessStatusCode();
        var program = (await programResp.Content.ReadFromJsonAsync<ProgramResponse>())!;

        // Family 1 + member 1 (enrolled, takes the one slot)
        var family1 = await CreateFamilyAsync("cap1fam1");
        await CreateCompletedMembershipAsync(family1.Id, MembershipType.Program);
        var m1Resp = await _adminClient.PostAsJsonAsync($"/api/v1/families/{family1.Id}/members",
            new { FullName = "Member One", DateOfBirth = new DateOnly(1990, 1, 1) });
        var m1 = await m1Resp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var member1Id = m1.GetProperty("id").GetGuid();
        var enrol1 = await _adminClient.PostAsJsonAsync($"/api/v1/programs/{program.Id}/enrolments",
            new EnrolMemberRequest(member1Id));
        var enrol1Body = (await enrol1.Content.ReadFromJsonAsync<EnrolmentResponse>())!;
        enrol1Body.Status.Should().Be(EnrolmentStatus.Enrolled);

        // Family 2 + member 2 (waitlisted — program full)
        var family2 = await CreateFamilyAsync("cap1fam2");
        await CreateCompletedMembershipAsync(family2.Id, MembershipType.Program);
        var m2Resp = await _adminClient.PostAsJsonAsync($"/api/v1/families/{family2.Id}/members",
            new { FullName = "Member Two", DateOfBirth = new DateOnly(1991, 1, 1) });
        var m2 = await m2Resp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var member2Id = m2.GetProperty("id").GetGuid();
        var enrol2 = await _adminClient.PostAsJsonAsync($"/api/v1/programs/{program.Id}/enrolments",
            new EnrolMemberRequest(member2Id));

        enrol2.StatusCode.Should().Be(HttpStatusCode.Created);
        var enrol2Body = (await enrol2.Content.ReadFromJsonAsync<EnrolmentResponse>())!;
        enrol2Body.Status.Should().Be(EnrolmentStatus.Waitlisted);
        enrol2Body.WaitlistPosition.Should().Be(1);
    }

    [Fact]
    public async Task CancelEnrolment_PromotesFirstWaitlisted_InSingleTransaction()
    {
        // Program with capacity 1
        var programResp = await _adminClient.PostAsJsonAsync("/api/v1/programs",
            new CreateProgramRequest("Promote Program", 1));
        var program = (await programResp.Content.ReadFromJsonAsync<ProgramResponse>())!;

        // Enrol member 1 (takes the slot)
        var fam1 = await CreateFamilyAsync("promote1");
        await CreateCompletedMembershipAsync(fam1.Id, MembershipType.Program);
        var m1El = await (await _adminClient.PostAsJsonAsync($"/api/v1/families/{fam1.Id}/members",
            new { FullName = "Enrolled Member", DateOfBirth = new DateOnly(1990, 1, 1) }))
            .Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var member1Id = m1El.GetProperty("id").GetGuid();
        var enrol1 = (await (await _adminClient.PostAsJsonAsync($"/api/v1/programs/{program.Id}/enrolments",
            new EnrolMemberRequest(member1Id))).Content.ReadFromJsonAsync<EnrolmentResponse>())!;

        // Enrol member 2 (waitlisted, position 1)
        var fam2 = await CreateFamilyAsync("promote2");
        await CreateCompletedMembershipAsync(fam2.Id, MembershipType.Program);
        var m2El = await (await _adminClient.PostAsJsonAsync($"/api/v1/families/{fam2.Id}/members",
            new { FullName = "Waitlisted Member", DateOfBirth = new DateOnly(1991, 1, 1) }))
            .Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var member2Id = m2El.GetProperty("id").GetGuid();
        var enrol2 = (await (await _adminClient.PostAsJsonAsync($"/api/v1/programs/{program.Id}/enrolments",
            new EnrolMemberRequest(member2Id))).Content.ReadFromJsonAsync<EnrolmentResponse>())!;
        enrol2.Status.Should().Be(EnrolmentStatus.Waitlisted);

        // Cancel enrolment 1 → member 2 should be promoted
        var cancelResp = await _adminClient.DeleteAsync(
            $"/api/v1/programs/{program.Id}/enrolments/{enrol1.Id}");
        cancelResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify in DB that member 2 is now Enrolled
        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SabandaDbContext>();
        var promoted = db.ProgramEnrolments.FirstOrDefault(e => e.Id == enrol2.Id);
        promoted.Should().NotBeNull();
        promoted!.Status.Should().Be(EnrolmentStatus.Enrolled);
        promoted.WaitlistPosition.Should().BeNull();
    }

    // ── Event registration tests ───────────────────────────────────────────────

    [Fact]
    public async Task RegisterEvent_AsFamilyMember_Returns403()
    {
        // Create a family and seed a FamilyMember user directly in DB
        var family = await CreateFamilyAsync("famrole");
        var fmEmail = $"fm_{Guid.NewGuid():N}@test.com";
        var fmPassword = "FamilyMember@1!";

        using (var scope = _fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SabandaDbContext>();
            var hash = BCrypt.Net.BCrypt.HashPassword(fmPassword, workFactor: 4);
            var fmUser = new AppUser(_fixture.TenantId, fmEmail, hash, UserRole.FamilyMember, family.Id);
            await db.AppUsers.AddAsync(fmUser);
            await db.SaveChangesAsync();
        }

        // Log in as FamilyMember
        var loginClient = _fixture.Factory.CreateClient();
        loginClient.DefaultRequestHeaders.Add("X-Tenant-Slug", _fixture.TenantSlug);
        var loginResp = await loginClient.PostAsJsonAsync("/api/v1/auth/login",
            new { Email = fmEmail, Password = fmPassword });
        var loginBody = (await loginResp.Content.ReadFromJsonAsync<LoginResponse>())!;

        var fmClient = _fixture.Factory.CreateClient();
        fmClient.DefaultRequestHeaders.Add("X-Tenant-Slug", _fixture.TenantSlug);
        fmClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginBody.Token);

        // Create an event and try to register as FamilyMember
        var eventResp = await _adminClient.PostAsJsonAsync("/api/v1/events",
            new CreateEventRequest("Family Event", DateTimeOffset.UtcNow.AddDays(30), 50, EventBillingType.Family));
        var @event = (await eventResp.Content.ReadFromJsonAsync<EventResponse>())!;

        var regResp = await fmClient.PostAsJsonAsync($"/api/v1/events/{@event.Id}/registrations",
            new RegisterEventRequest(family.Id));

        regResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RegisterEvent_WhenAtCapacity_Returns201WithWaitlistedStatus()
    {
        // Create an event with capacity 1
        var eventResp = await _adminClient.PostAsJsonAsync("/api/v1/events",
            new CreateEventRequest("Small Event", DateTimeOffset.UtcNow.AddDays(30), 1, EventBillingType.Family));
        var @event = (await eventResp.Content.ReadFromJsonAsync<EventResponse>())!;

        // Family 1 registers (takes the slot)
        var fam1 = await CreateFamilyAsync("evtcap1");
        await CreateCompletedMembershipAsync(fam1.Id, MembershipType.Event);
        var reg1 = await _adminClient.PostAsJsonAsync($"/api/v1/events/{@event.Id}/registrations",
            new RegisterEventRequest(fam1.Id));
        var reg1Body = (await reg1.Content.ReadFromJsonAsync<RegistrationResponse>())!;
        reg1Body.Status.Should().Be(RegistrationStatus.Registered);

        // Family 2 registers (waitlisted)
        var fam2 = await CreateFamilyAsync("evtcap2");
        await CreateCompletedMembershipAsync(fam2.Id, MembershipType.Event);
        var reg2 = await _adminClient.PostAsJsonAsync($"/api/v1/events/{@event.Id}/registrations",
            new RegisterEventRequest(fam2.Id));

        reg2.StatusCode.Should().Be(HttpStatusCode.Created);
        var reg2Body = (await reg2.Content.ReadFromJsonAsync<RegistrationResponse>())!;
        reg2Body.Status.Should().Be(RegistrationStatus.Waitlisted);
        reg2Body.WaitlistPosition.Should().Be(1);
    }
}

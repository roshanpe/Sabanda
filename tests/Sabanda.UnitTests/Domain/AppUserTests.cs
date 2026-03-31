using FluentAssertions;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;
using Xunit;

namespace Sabanda.UnitTests.Domain;

public class AppUserTests
{
    private readonly DateTimeOffset _now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private AppUser MakeUser() =>
        new(Guid.NewGuid(), "user@test.com", "hash", UserRole.FamilyMember);

    [Fact]
    public void NewUser_StartsWith_ZeroFailedLogins()
    {
        var user = MakeUser();
        user.FailedLoginCount.Should().Be(0);
        user.LockedUntil.Should().BeNull();
    }

    [Fact]
    public void RecordFailedLogin_IncrementsCount()
    {
        var user = MakeUser();
        user.RecordFailedLogin(_now);
        user.FailedLoginCount.Should().Be(1);
        user.LockedUntil.Should().BeNull();
    }

    [Fact]
    public void RecordFailedLogin_FifthAttempt_SetsLockout()
    {
        var user = MakeUser();
        for (int i = 0; i < 5; i++)
            user.RecordFailedLogin(_now);

        user.IsLockedOut(_now.AddSeconds(1)).Should().BeTrue();
        user.LockedUntil.Should().Be(_now.AddMinutes(15));
    }

    [Fact]
    public void IsLockedOut_ReturnsFalse_WhenLockExpired()
    {
        var user = MakeUser();
        for (int i = 0; i < 5; i++)
            user.RecordFailedLogin(_now);

        user.IsLockedOut(_now.AddMinutes(16)).Should().BeFalse();
    }

    [Fact]
    public void RecordSuccessfulLogin_ResetsState()
    {
        var user = MakeUser();
        user.RecordFailedLogin(_now);
        user.RecordFailedLogin(_now);
        user.RecordSuccessfulLogin();

        user.FailedLoginCount.Should().Be(0);
        user.LockedUntil.Should().BeNull();
    }
}

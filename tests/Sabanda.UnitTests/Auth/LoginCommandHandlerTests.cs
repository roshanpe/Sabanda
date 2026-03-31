using FluentAssertions;
using NSubstitute;
using Sabanda.Application.Auth.Commands;
using Sabanda.Application.Auth.DTOs;
using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;
using Xunit;

namespace Sabanda.UnitTests.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly IAuditLogService _auditLog = Substitute.For<IAuditLogService>();
    private readonly ICurrentTenantService _tenant = Substitute.For<ICurrentTenantService>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();

    private readonly LoginCommandHandler _sut;
    private readonly DateTimeOffset _now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
    private readonly Guid _tenantId = Guid.NewGuid();

    public LoginCommandHandlerTests()
    {
        _clock.UtcNow.Returns(_now);
        _tenant.TenantId.Returns(_tenantId);
        _sut = new LoginCommandHandler(_userRepo, _tokenService, _auditLog, _tenant, _clock, _hasher);
    }

    private AppUser MakeUser(string email = "user@test.com") =>
        new(_tenantId, email, "hashed_password", UserRole.PrimaryAccountHolder);

    [Fact]
    public async Task ValidCredentials_ReturnsLoginResponse()
    {
        var user = MakeUser();
        _userRepo.FindByEmailAsync(_tenantId, "user@test.com").Returns(user);
        _hasher.Verify("password", "hashed_password").Returns(true);
        _tokenService.IssueTokenAsync(user).Returns("jwt_token");

        var result = await _sut.HandleAsync(new LoginRequest("user@test.com", "password"));

        result.Token.Should().Be("jwt_token");
        result.Role.Should().Be(UserRole.PrimaryAccountHolder);
    }

    [Fact]
    public async Task WrongPassword_IncrementsFailedLoginCount()
    {
        var user = MakeUser();
        _userRepo.FindByEmailAsync(_tenantId, "user@test.com").Returns(user);
        _hasher.Verify("wrong", "hashed_password").Returns(false);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.HandleAsync(new LoginRequest("user@test.com", "wrong")));

        user.FailedLoginCount.Should().Be(1);
        await _userRepo.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task FifthFailure_SetsLockout()
    {
        var user = MakeUser();
        _userRepo.FindByEmailAsync(_tenantId, "user@test.com").Returns(user);
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        for (int i = 0; i < 5; i++)
        {
            await Assert.ThrowsAsync<UnauthorizedException>(() =>
                _sut.HandleAsync(new LoginRequest("user@test.com", "wrong")));
        }

        user.FailedLoginCount.Should().Be(5);
        user.LockedUntil.Should().NotBeNull();
        user.LockedUntil!.Value.Should().Be(_now.AddMinutes(15));
    }

    [Fact]
    public async Task LockedAccount_DoesNotCallPasswordHasher()
    {
        var user = MakeUser();
        // Force lockout state
        for (int i = 0; i < 5; i++)
            user.RecordFailedLogin(_now);

        _userRepo.FindByEmailAsync(_tenantId, "user@test.com").Returns(user);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.HandleAsync(new LoginRequest("user@test.com", "any")));

        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task SuccessfulLogin_ResetsFailedLoginCount()
    {
        var user = MakeUser();
        // Simulate 3 prior failures
        user.RecordFailedLogin(_now);
        user.RecordFailedLogin(_now);
        user.RecordFailedLogin(_now);

        _userRepo.FindByEmailAsync(_tenantId, "user@test.com").Returns(user);
        _hasher.Verify("password", "hashed_password").Returns(true);
        _tokenService.IssueTokenAsync(user).Returns("token");

        await _sut.HandleAsync(new LoginRequest("user@test.com", "password"));

        user.FailedLoginCount.Should().Be(0);
        user.LockedUntil.Should().BeNull();
    }

    [Fact]
    public async Task UserNotFound_ThrowsUnauthorizedWithGenericMessage()
    {
        _userRepo.FindByEmailAsync(_tenantId, "nobody@test.com").Returns((AppUser?)null);

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _sut.HandleAsync(new LoginRequest("nobody@test.com", "pass")));

        ex.Message.Should().Be("Invalid credentials.");
        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task SuccessfulLogin_WritesAuditLogAndSaves()
    {
        var user = MakeUser();
        _userRepo.FindByEmailAsync(_tenantId, "user@test.com").Returns(user);
        _hasher.Verify("password", "hashed_password").Returns(true);
        _tokenService.IssueTokenAsync(user).Returns("token");

        await _sut.HandleAsync(new LoginRequest("user@test.com", "password"));

        await _auditLog.Received(1).LogAsync(
            AuditAction.Login,
            "AppUser",
            user.Id,
            Arg.Any<object?>());
        await _userRepo.Received(1).SaveChangesAsync();
    }
}

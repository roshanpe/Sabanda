using Sabanda.Application.Auth.DTOs;
using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Enums;

namespace Sabanda.Application.Auth.Commands;

public class LoginCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentTenantService _tenant;
    private readonly IDateTimeProvider _clock;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IAuditLogService auditLog,
        ICurrentTenantService tenant,
        IDateTimeProvider clock,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _auditLog = auditLog;
        _tenant = tenant;
        _clock = clock;
        _passwordHasher = passwordHasher;
    }

    public async Task<LoginResponse> HandleAsync(LoginRequest request)
    {
        var now = _clock.UtcNow;
        var user = await _userRepository.FindByEmailAsync(_tenant.TenantId, request.Email);

        if (user == null)
        {
            // Don't reveal that user doesn't exist — same response as wrong password
            await _auditLog.LogAsync(AuditAction.LoginFailed, detail: new { attemptedEmail = request.Email });
            await _userRepository.SaveChangesAsync();
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (user.IsLockedOut(now))
        {
            await _auditLog.LogAsync(AuditAction.LoginFailed, "AppUser", user.Id, new { reason = "AccountLocked" });
            await _userRepository.SaveChangesAsync();
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin(now);
            await _auditLog.LogAsync(AuditAction.LoginFailed, "AppUser", user.Id, new { reason = "WrongPassword" });
            await _userRepository.SaveChangesAsync();
            throw new UnauthorizedException("Invalid credentials.");
        }

        user.RecordSuccessfulLogin();
        var token = await _tokenService.IssueTokenAsync(user);
        await _auditLog.LogAsync(AuditAction.Login, "AppUser", user.Id);
        await _userRepository.SaveChangesAsync();

        return new LoginResponse(token, now.AddHours(8), user.Id, user.Role, user.FamilyId);
    }
}

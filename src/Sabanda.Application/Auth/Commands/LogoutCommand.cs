using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Enums;

namespace Sabanda.Application.Auth.Commands;

public class LogoutCommandHandler
{
    private readonly ITokenService _tokenService;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _userRepository;

    public LogoutCommandHandler(
        ITokenService tokenService,
        IAuditLogService auditLog,
        ICurrentUserService currentUser,
        IUserRepository userRepository)
    {
        _tokenService = tokenService;
        _auditLog = auditLog;
        _currentUser = currentUser;
        _userRepository = userRepository;
    }

    public async Task HandleAsync(string jti)
    {
        await _tokenService.RevokeAsync(jti);
        await _auditLog.LogAsync(AuditAction.Logout, "AppUser", _currentUser.UserId);
        await _userRepository.SaveChangesAsync();
    }
}

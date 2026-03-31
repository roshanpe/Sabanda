using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Enums;

namespace Sabanda.Application.Qr.Commands;

public class RegenerateMemberQrCommandHandler
{
    private readonly IMemberRepository _memberRepository;
    private readonly IQrTokenService _qrTokenService;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentUserService _currentUser;

    public RegenerateMemberQrCommandHandler(
        IMemberRepository memberRepository,
        IQrTokenService qrTokenService,
        IAuditLogService auditLog,
        ICurrentTenantService tenant,
        ICurrentUserService currentUser)
    {
        _memberRepository = memberRepository;
        _qrTokenService = qrTokenService;
        _auditLog = auditLog;
        _tenant = tenant;
        _currentUser = currentUser;
    }

    public async Task<string> HandleAsync(Guid memberId)
    {
        var member = await _memberRepository.FindByIdAsync(memberId)
            ?? throw new NotFoundException($"Member {memberId} not found.");

        if (_currentUser.Role != UserRole.Administrator
            && _currentUser.FamilyId != member.FamilyId)
        {
            throw new ForbiddenException("Access denied.");
        }

        var result = await _qrTokenService.IssueAsync(member.Id, "member", _tenant.TenantId);
        member.SetQrToken(result.Token, result.Jti);

        await _auditLog.LogAsync(AuditAction.QrTokenRegenerated, "Member", member.Id);
        await _memberRepository.SaveChangesAsync();

        return result.Token;
    }
}

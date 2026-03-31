using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Enums;

namespace Sabanda.Application.Qr.Commands;

public class RegenerateFamilyQrCommandHandler
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IQrTokenService _qrTokenService;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentUserService _currentUser;

    public RegenerateFamilyQrCommandHandler(
        IFamilyRepository familyRepository,
        IQrTokenService qrTokenService,
        IAuditLogService auditLog,
        ICurrentTenantService tenant,
        ICurrentUserService currentUser)
    {
        _familyRepository = familyRepository;
        _qrTokenService = qrTokenService;
        _auditLog = auditLog;
        _tenant = tenant;
        _currentUser = currentUser;
    }

    public async Task<string> HandleAsync(Guid familyId)
    {
        var family = await _familyRepository.FindByIdAsync(familyId)
            ?? throw new NotFoundException($"Family {familyId} not found.");

        if (_currentUser.Role != UserRole.Administrator
            && _currentUser.FamilyId != family.Id)
        {
            throw new ForbiddenException("Access denied.");
        }

        var result = await _qrTokenService.IssueAsync(family.Id, "family", _tenant.TenantId);
        family.SetQrToken(result.Token, result.Jti);

        await _auditLog.LogAsync(AuditAction.QrTokenRegenerated, "Family", family.Id);
        await _familyRepository.SaveChangesAsync();

        return result.Token;
    }
}

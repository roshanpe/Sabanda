using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Application.Families.DTOs;
using Sabanda.Application.Members.Commands;
using Sabanda.Application.Members.DTOs;
using Sabanda.Domain.Enums;

namespace Sabanda.Application.Qr.Queries;

public record QrLookupResponse(
    string SubjectType,
    Guid SubjectId,
    FamilyResponse? Family,
    MemberResponse? Member
);

public class QrLookupQueryHandler
{
    private readonly IQrTokenService _qrTokenService;
    private readonly IFamilyRepository _familyRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentTenantService _tenant;

    public QrLookupQueryHandler(
        IQrTokenService qrTokenService,
        IFamilyRepository familyRepository,
        IMemberRepository memberRepository,
        IAuditLogService auditLog,
        ICurrentTenantService tenant)
    {
        _qrTokenService = qrTokenService;
        _familyRepository = familyRepository;
        _memberRepository = memberRepository;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<QrLookupResponse> HandleAsync(string token)
    {
        // ValidateAndLookup throws UnauthorizedException if the token is invalid or expired
        var lookup = _qrTokenService.ValidateAndLookup(token);

        // Tenant isolation: reject tokens issued for a different tenant
        if (lookup.TenantId != _tenant.TenantId)
            throw new ForbiddenException("QR token belongs to a different tenant.");

        if (lookup.SubjectType == "family")
        {
            var family = await _familyRepository.FindByQrTokenJtiAsync(lookup.Jti)
                ?? throw new NotFoundException("QR token has been regenerated or is no longer valid.");

            var response = new FamilyResponse(family.Id, family.DisplayName,
                family.PrimaryHolderUserId, family.QrToken != null, family.CreatedAt);

            await _auditLog.LogAsync(AuditAction.QrTokenRegenerated, "Family", family.Id);
            await _familyRepository.SaveChangesAsync();

            return new QrLookupResponse("family", family.Id, response, null);
        }
        else if (lookup.SubjectType == "member")
        {
            var member = await _memberRepository.FindByQrTokenJtiAsync(lookup.Jti)
                ?? throw new NotFoundException("QR token has been regenerated or is no longer valid.");

            if (!member.IsAdult)
            {
                await _auditLog.LogAsync(AuditAction.MinorDataRead, "Member", member.Id);
                await _memberRepository.SaveChangesAsync();
            }

            return new QrLookupResponse("member", member.Id, null,
                CreateMemberCommandHandler.ToResponse(member));
        }
        else
        {
            throw new NotFoundException("Unknown QR token subject type.");
        }
    }
}

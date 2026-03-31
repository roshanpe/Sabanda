using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Application.Members.Commands;
using Sabanda.Application.Members.DTOs;
using Sabanda.Domain.Enums;

namespace Sabanda.Application.Members.Queries;

public class GetMemberQueryHandler
{
    private readonly IMemberRepository _memberRepository;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentUserService _currentUser;

    public GetMemberQueryHandler(
        IMemberRepository memberRepository,
        IAuditLogService auditLog,
        ICurrentUserService currentUser)
    {
        _memberRepository = memberRepository;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    public async Task<MemberResponse> HandleAsync(Guid memberId)
    {
        var member = await _memberRepository.FindByIdAsync(memberId)
            ?? throw new NotFoundException($"Member {memberId} not found.");

        if (_currentUser.Role != UserRole.Administrator
            && _currentUser.Role != UserRole.ProgramCoordinator
            && _currentUser.Role != UserRole.EventCoordinator
            && _currentUser.FamilyId != member.FamilyId)
        {
            throw new ForbiddenException("Access denied.");
        }

        if (!member.IsAdult)
            await _auditLog.LogAsync(AuditAction.MinorDataRead, "Member", member.Id);

        // Persist the audit log — SaveChanges is needed since the caller doesn't have a repo reference
        await _memberRepository.SaveChangesAsync();

        return CreateMemberCommandHandler.ToResponse(member);
    }
}

using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Application.Families.DTOs;
using Sabanda.Domain.Enums;

namespace Sabanda.Application.Families.Queries;

public class GetFamilyQueryHandler
{
    private readonly IFamilyRepository _familyRepository;
    private readonly ICurrentUserService _currentUser;

    public GetFamilyQueryHandler(IFamilyRepository familyRepository, ICurrentUserService currentUser)
    {
        _familyRepository = familyRepository;
        _currentUser = currentUser;
    }

    public async Task<FamilyResponse> HandleAsync(Guid familyId)
    {
        var family = await _familyRepository.FindByIdAsync(familyId)
            ?? throw new NotFoundException($"Family {familyId} not found.");

        // Admin can access any family; PrimaryAccountHolder can only access their own
        if (_currentUser.Role != UserRole.Administrator
            && _currentUser.FamilyId != family.Id)
        {
            throw new ForbiddenException("Access denied.");
        }

        return new FamilyResponse(family.Id, family.DisplayName, family.PrimaryHolderUserId,
            family.QrToken != null, family.CreatedAt);
    }
}

public class GetFamilySummaryQueryHandler
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ICurrentUserService _currentUser;

    public GetFamilySummaryQueryHandler(
        IFamilyRepository familyRepository,
        IMemberRepository memberRepository,
        ICurrentUserService currentUser)
    {
        _familyRepository = familyRepository;
        _memberRepository = memberRepository;
        _currentUser = currentUser;
    }

    public async Task<FamilySummaryResponse> HandleAsync(Guid familyId)
    {
        var family = await _familyRepository.FindByIdAsync(familyId)
            ?? throw new NotFoundException($"Family {familyId} not found.");

        if (_currentUser.Role != UserRole.Administrator
            && _currentUser.Role != UserRole.ProgramCoordinator
            && _currentUser.Role != UserRole.EventCoordinator
            && _currentUser.FamilyId != family.Id)
        {
            throw new ForbiddenException("Access denied.");
        }

        var memberCount = await _memberRepository.CountByFamilyIdAsync(familyId);

        return new FamilySummaryResponse(family.Id, family.DisplayName,
            family.PrimaryHolderUserId, memberCount, family.CreatedAt);
    }
}

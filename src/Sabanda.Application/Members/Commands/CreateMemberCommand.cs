using FluentValidation;
using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Application.Members.DTOs;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;

namespace Sabanda.Application.Members.Commands;

public class CreateMemberCommandHandler
{
    private readonly IMemberRepository _memberRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateMemberRequest> _validator;

    public CreateMemberCommandHandler(
        IMemberRepository memberRepository,
        IFamilyRepository familyRepository,
        IAuditLogService auditLog,
        ICurrentTenantService tenant,
        ICurrentUserService currentUser,
        IValidator<CreateMemberRequest> validator)
    {
        _memberRepository = memberRepository;
        _familyRepository = familyRepository;
        _auditLog = auditLog;
        _tenant = tenant;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<MemberResponse> HandleAsync(Guid familyId, CreateMemberRequest request)
    {
        var result = await _validator.ValidateAsync(request);
        if (!result.IsValid)
            throw new Common.Exceptions.ValidationException(
                result.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var family = await _familyRepository.FindByIdAsync(familyId)
            ?? throw new NotFoundException($"Family {familyId} not found.");

        // Admin or the family's PrimaryAccountHolder can add members
        if (_currentUser.Role != UserRole.Administrator
            && _currentUser.FamilyId != family.Id)
        {
            throw new ForbiddenException("Access denied.");
        }

        var member = new Member(_tenant.TenantId, familyId, request.FullName, request.DateOfBirth,
            request.Gender, request.Email, request.Phone, request.IsPrimaryHolder);

        if (!member.IsAdult && request.ConsentGiven == true)
        {
            member.GrantConsent(request.ConsentGivenBy!.Value, request.ConsentGivenAt!.Value);
        }

        if (member.IsAdult)
        {
            if (request.Occupation != null || request.BusinessName != null)
                member.SetAdultFields(request.Occupation, request.BusinessName);
        }

        await _memberRepository.AddAsync(member);
        await _auditLog.LogAsync(AuditAction.MemberCreated, "Member", member.Id,
            new { familyId, fullName = request.FullName, isAdult = member.IsAdult });
        await _memberRepository.SaveChangesAsync();

        return ToResponse(member);
    }

    internal static MemberResponse ToResponse(Member m) =>
        new(m.Id, m.FamilyId, m.FullName, m.DateOfBirth, m.IsAdult, m.Gender,
            m.Email, m.Phone, m.IsPrimaryHolder, m.ConsentGiven,
            m.Occupation, m.BusinessName, m.QrToken != null, m.CreatedAt);
}

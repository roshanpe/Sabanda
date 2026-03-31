using FluentValidation;
using Sabanda.Application.Common.Exceptions;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Application.Memberships.DTOs;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;

namespace Sabanda.Application.Memberships.Commands;

public class CreateMembershipCommandHandler
{
    private readonly IMembershipRepository _membershipRepo;
    private readonly ICurrentTenantService _tenant;
    private readonly IValidator<CreateMembershipRequest> _validator;

    public CreateMembershipCommandHandler(
        IMembershipRepository membershipRepo,
        ICurrentTenantService tenant,
        IValidator<CreateMembershipRequest> validator)
    {
        _membershipRepo = membershipRepo;
        _tenant = tenant;
        _validator = validator;
    }

    public async Task<MembershipResponse> HandleAsync(CreateMembershipRequest request)
    {
        var result = await _validator.ValidateAsync(request);
        if (!result.IsValid)
            throw new Common.Exceptions.ValidationException(
                result.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

        var hasOverlap = await _membershipRepo.HasOverlapAsync(
            request.FamilyId, request.Type, request.StartDate, request.EndDate, request.MemberId);
        if (hasOverlap)
            throw new ConflictException(
                "An overlapping non-refunded membership of this type already exists.");

        var membership = new Membership(_tenant.TenantId, request.FamilyId, request.Type,
            request.StartDate, request.EndDate, request.MemberId);

        await _membershipRepo.AddAsync(membership);
        await _membershipRepo.SaveChangesAsync();

        return ToResponse(membership);
    }

    internal static MembershipResponse ToResponse(Membership m) =>
        new(m.Id, m.FamilyId, m.MemberId, m.Type, m.StartDate, m.EndDate,
            m.PaymentStatus, m.CreatedAt);
}

public class UpdatePaymentStatusCommandHandler
{
    private readonly IMembershipRepository _membershipRepo;

    public UpdatePaymentStatusCommandHandler(IMembershipRepository membershipRepo)
    {
        _membershipRepo = membershipRepo;
    }

    public async Task<MembershipResponse> HandleAsync(Guid membershipId, UpdatePaymentStatusRequest request)
    {
        var membership = await _membershipRepo.FindByIdAsync(membershipId)
            ?? throw new NotFoundException($"Membership {membershipId} not found.");

        if (!membership.CanTransitionTo(request.NewStatus))
            throw new Common.Exceptions.ValidationException(
                new Dictionary<string, string[]>
                {
                    ["NewStatus"] = [$"Cannot transition from {membership.PaymentStatus} to {request.NewStatus}."]
                });

        membership.UpdatePaymentStatus(request.NewStatus);
        await _membershipRepo.SaveChangesAsync();

        return CreateMembershipCommandHandler.ToResponse(membership);
    }
}

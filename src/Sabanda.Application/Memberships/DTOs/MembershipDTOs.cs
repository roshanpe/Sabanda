using Sabanda.Domain.Enums;

namespace Sabanda.Application.Memberships.DTOs;

public record CreateMembershipRequest(
    Guid FamilyId,
    MembershipType Type,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? MemberId = null
);

public record UpdatePaymentStatusRequest(PaymentStatus NewStatus);

public record MembershipResponse(
    Guid Id,
    Guid FamilyId,
    Guid? MemberId,
    MembershipType Type,
    DateOnly StartDate,
    DateOnly EndDate,
    PaymentStatus PaymentStatus,
    DateTimeOffset CreatedAt
);

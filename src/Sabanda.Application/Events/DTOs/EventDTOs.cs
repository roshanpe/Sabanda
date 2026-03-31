using Sabanda.Domain.Enums;

namespace Sabanda.Application.Events.DTOs;

public record CreateEventRequest(
    string Name,
    DateTimeOffset EventDate,
    int Capacity,
    EventBillingType BillingType,
    string? Description = null,
    Guid? CoordinatorUserId = null
);

public record EventResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset EventDate,
    int Capacity,
    EventBillingType BillingType,
    Guid? CoordinatorUserId,
    DateTimeOffset CreatedAt
);

public record RegisterEventRequest(
    Guid FamilyId,
    Guid? MemberId = null
);

public record RegistrationResponse(
    Guid Id,
    Guid EventId,
    Guid FamilyId,
    Guid? MemberId,
    RegistrationStatus Status,
    int? WaitlistPosition,
    DateTimeOffset RegisteredAt,
    DateTimeOffset? CancelledAt
);

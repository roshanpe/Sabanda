using Sabanda.Domain.Enums;

namespace Sabanda.Application.Programs.DTOs;

public record CreateProgramRequest(
    string Name,
    int Capacity,
    string? Description = null,
    Guid? CoordinatorUserId = null
);

public record ProgramResponse(
    Guid Id,
    string Name,
    string? Description,
    int Capacity,
    Guid? CoordinatorUserId,
    DateTimeOffset CreatedAt
);

public record EnrolMemberRequest(Guid MemberId);

public record EnrolmentResponse(
    Guid Id,
    Guid ProgramId,
    Guid MemberId,
    EnrolmentStatus Status,
    int? WaitlistPosition,
    DateTimeOffset EnrolledAt,
    DateTimeOffset? CancelledAt
);

using Sabanda.Domain.Enums;

namespace Sabanda.Application.Programs.DTOs;

public record CreateProgramRequest(
    string Name,
    int Capacity,
    string? Description = null,
    Guid? CoordinatorUserId = null,
    string? AgeGroup = null,
    Frequency? Frequency = null,
    string? Venue = null,
    DayOfWeek? Day = null,
    TimeOnly? Time = null
);

public record ProgramResponse(
    Guid Id,
    string Name,
    string? Description,
    int Capacity,
    Guid? CoordinatorUserId,
    string? AgeGroup,
    Frequency? Frequency,
    string? Venue,
    DayOfWeek? Day,
    TimeOnly? Time,
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

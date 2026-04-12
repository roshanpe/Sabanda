namespace Sabanda.Application.Members.DTOs;

public record CreateMemberRequest(
    string FullName,
    DateOnly DateOfBirth,
    string? Gender = null,
    string? Email = null,
    string? Phone = null,
    bool IsPrimaryHolder = false,
    bool? ConsentGiven = null,
    Guid? ConsentGivenBy = null,
    DateTimeOffset? ConsentGivenAt = null,
    string? Occupation = null,
    string? BusinessName = null
);

public record MemberResponse(
    Guid Id,
    Guid FamilyId,
    string FullName,
    string Code,
    DateOnly DateOfBirth,
    bool IsAdult,
    string? Gender,
    string? Email,
    string? Phone,
    bool IsPrimaryHolder,
    bool ConsentGiven,
    string? Occupation,
    string? BusinessName,
    bool HasQrToken,
    DateTimeOffset CreatedAt
);

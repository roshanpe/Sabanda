namespace Sabanda.Application.Families.DTOs;

public record CreateFamilyRequest(
    string DisplayName,
    string PrimaryHolderEmail,
    string PrimaryHolderPassword
);

public record FamilyResponse(
    Guid Id,
    string DisplayName,
    Guid PrimaryHolderUserId,
    bool HasQrToken,
    DateTimeOffset CreatedAt
);

public record FamilySummaryResponse(
    Guid Id,
    string DisplayName,
    Guid PrimaryHolderUserId,
    int MemberCount,
    DateTimeOffset CreatedAt
);

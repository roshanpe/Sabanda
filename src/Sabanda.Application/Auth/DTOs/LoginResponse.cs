using Sabanda.Domain.Enums;

namespace Sabanda.Application.Auth.DTOs;

public record LoginResponse(
    string Token,
    DateTimeOffset ExpiresAt,
    Guid UserId,
    UserRole Role,
    Guid? FamilyId);

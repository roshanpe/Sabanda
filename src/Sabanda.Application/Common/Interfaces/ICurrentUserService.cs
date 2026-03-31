using Sabanda.Domain.Enums;

namespace Sabanda.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    UserRole Role { get; }
    Guid? FamilyId { get; }
    bool IsAuthenticated { get; }
}

using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;

namespace Sabanda.Application.Common.Interfaces;

public interface IMembershipRepository
{
    Task<Membership?> FindByIdAsync(Guid id);
    Task<bool> HasOverlapAsync(Guid familyId, MembershipType type, DateOnly start, DateOnly end, Guid? memberId = null);
    Task<Membership?> FindActiveAsync(Guid familyId, MembershipType type, DateOnly today);
    Task AddAsync(Membership membership);
    Task SaveChangesAsync();
}

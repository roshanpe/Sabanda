using Sabanda.Domain.Entities;

namespace Sabanda.Application.Common.Interfaces;

public interface IMemberRepository
{
    Task<Member?> FindByIdAsync(Guid id);
    Task<Member?> FindByQrTokenJtiAsync(Guid jti);
    Task<bool> ExistsByCodeAsync(Guid tenantId, string code);
    Task<int> CountByFamilyIdAsync(Guid familyId);
    Task AddAsync(Member member);
    Task SaveChangesAsync();
}

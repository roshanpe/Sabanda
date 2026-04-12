using Sabanda.Domain.Entities;

namespace Sabanda.Application.Common.Interfaces;

public interface IFamilyRepository
{
    Task<Family?> FindByIdAsync(Guid id);
    Task<Family?> FindByQrTokenJtiAsync(Guid jti);
    Task<bool> ExistsByCodeAsync(Guid tenantId, string code);
    Task AddAsync(Family family);
    Task SaveChangesAsync();
}

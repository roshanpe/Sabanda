using Sabanda.Domain.Entities;

namespace Sabanda.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> FindByEmailAsync(Guid tenantId, string email);
    Task<AppUser?> FindByIdAsync(Guid id);
    Task AddAsync(AppUser user);
    Task SaveChangesAsync();
}

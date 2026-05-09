using Sabanda.Domain.Entities;
using Program = Sabanda.Domain.Entities.Program;

namespace Sabanda.Application.Common.Interfaces;

public interface IProgramRepository
{
    Task<Program?> FindByIdAsync(Guid id);
    Task<List<Program>> GetAllByTenantAsync(Guid tenantId);
    Task AddAsync(Program program);
    Task SaveChangesAsync();
}

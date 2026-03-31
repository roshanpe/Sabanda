using Microsoft.EntityFrameworkCore;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Entities;
using Sabanda.Infrastructure.Persistence;

namespace Sabanda.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SabandaDbContext _db;

    public UserRepository(SabandaDbContext db)
    {
        _db = db;
    }

    public async Task<AppUser?> FindByEmailAsync(Guid tenantId, string email)
    {
        // Bypass global query filter to find user during login (tenant filter is based on email domain lookup)
        return await _db.AppUsers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email.ToLowerInvariant());
    }

    public async Task<AppUser?> FindByIdAsync(Guid id)
    {
        return await _db.AppUsers.FindAsync(id);
    }

    public async Task AddAsync(AppUser user)
    {
        await _db.AppUsers.AddAsync(user);
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}

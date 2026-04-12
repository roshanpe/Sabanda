using Microsoft.EntityFrameworkCore;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Entities;
using Sabanda.Infrastructure.Persistence;

namespace Sabanda.Infrastructure.Repositories;

public class FamilyRepository : IFamilyRepository
{
    private readonly SabandaDbContext _db;

    public FamilyRepository(SabandaDbContext db)
    {
        _db = db;
    }

    public Task<Family?> FindByIdAsync(Guid id) =>
        _db.Families.FirstOrDefaultAsync(f => f.Id == id);

    public Task<Family?> FindByQrTokenJtiAsync(Guid jti) =>
        _db.Families.FirstOrDefaultAsync(f => f.QrTokenJti == jti);

    public Task<bool> ExistsByCodeAsync(Guid tenantId, string code) =>
        _db.Families.AnyAsync(f => f.TenantId == tenantId && f.Code == code);

    public async Task AddAsync(Family family) =>
        await _db.Families.AddAsync(family);

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}

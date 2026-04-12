using Microsoft.EntityFrameworkCore;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Entities;
using Sabanda.Infrastructure.Persistence;

namespace Sabanda.Infrastructure.Repositories;

public class MemberRepository : IMemberRepository
{
    private readonly SabandaDbContext _db;

    public MemberRepository(SabandaDbContext db)
    {
        _db = db;
    }

    public Task<Member?> FindByIdAsync(Guid id) =>
        _db.Members.FirstOrDefaultAsync(m => m.Id == id);

    public Task<Member?> FindByQrTokenJtiAsync(Guid jti) =>
        _db.Members.FirstOrDefaultAsync(m => m.QrTokenJti == jti);

    public Task<int> CountByFamilyIdAsync(Guid familyId) =>
        _db.Members.CountAsync(m => m.FamilyId == familyId);

    public Task<bool> ExistsByCodeAsync(Guid tenantId, string code) =>
        _db.Members.AnyAsync(m => m.TenantId == tenantId && m.Code == code);

    public async Task AddAsync(Member member) =>
        await _db.Members.AddAsync(member);

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}

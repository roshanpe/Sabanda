using Microsoft.EntityFrameworkCore;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;
using Sabanda.Infrastructure.Persistence;

namespace Sabanda.Infrastructure.Repositories;

public class MembershipRepository : IMembershipRepository
{
    private readonly SabandaDbContext _db;

    public MembershipRepository(SabandaDbContext db)
    {
        _db = db;
    }

    public Task<Membership?> FindByIdAsync(Guid id) =>
        _db.Memberships.FirstOrDefaultAsync(m => m.Id == id);

    public Task<bool> HasOverlapAsync(Guid familyId, MembershipType type, DateOnly start, DateOnly end, Guid? memberId = null) =>
        _db.Memberships.AnyAsync(m =>
            m.FamilyId == familyId &&
            m.Type == type &&
            m.PaymentStatus != PaymentStatus.Refunded &&
            m.StartDate <= end &&
            m.EndDate >= start &&
            (memberId == null || m.MemberId == memberId));

    public Task<Membership?> FindActiveAsync(Guid familyId, MembershipType type, DateOnly today) =>
        _db.Memberships.FirstOrDefaultAsync(m =>
            m.FamilyId == familyId &&
            m.Type == type &&
            m.PaymentStatus == PaymentStatus.Completed &&
            m.StartDate <= today &&
            m.EndDate >= today);

    public async Task AddAsync(Membership membership) =>
        await _db.Memberships.AddAsync(membership);

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}

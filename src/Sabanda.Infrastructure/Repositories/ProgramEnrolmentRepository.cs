using Microsoft.EntityFrameworkCore;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;
using Sabanda.Infrastructure.Persistence;

namespace Sabanda.Infrastructure.Repositories;

public class ProgramEnrolmentRepository : IProgramEnrolmentRepository
{
    private readonly SabandaDbContext _db;

    public ProgramEnrolmentRepository(SabandaDbContext db)
    {
        _db = db;
    }

    public Task<ProgramEnrolment?> FindByIdAsync(Guid id) =>
        _db.ProgramEnrolments.FirstOrDefaultAsync(e => e.Id == id);

    public Task<bool> IsMemberEnrolledOrWaitlistedAsync(Guid programId, Guid memberId) =>
        _db.ProgramEnrolments.AnyAsync(e =>
            e.ProgramId == programId &&
            e.MemberId == memberId &&
            e.Status != EnrolmentStatus.Cancelled);

    public Task<int> CountEnrolledAsync(Guid programId) =>
        _db.ProgramEnrolments.CountAsync(e =>
            e.ProgramId == programId &&
            e.Status == EnrolmentStatus.Enrolled);

    public async Task<int> GetMaxWaitlistPositionAsync(Guid programId)
    {
        var max = await _db.ProgramEnrolments
            .Where(e => e.ProgramId == programId && e.Status == EnrolmentStatus.Waitlisted)
            .MaxAsync(e => (int?)e.WaitlistPosition);
        return max ?? 0;
    }

    public Task<ProgramEnrolment?> GetFirstWaitlistedAsync(Guid programId) =>
        _db.ProgramEnrolments
            .Where(e => e.ProgramId == programId && e.Status == EnrolmentStatus.Waitlisted)
            .OrderBy(e => e.WaitlistPosition)
            .FirstOrDefaultAsync();

    public Task<List<ProgramEnrolment>> GetWaitlistedAfterPositionAsync(Guid programId, int afterPosition) =>
        _db.ProgramEnrolments
            .Where(e =>
                e.ProgramId == programId &&
                e.Status == EnrolmentStatus.Waitlisted &&
                e.WaitlistPosition > afterPosition)
            .ToListAsync();

    public async Task AddAsync(ProgramEnrolment enrolment) =>
        await _db.ProgramEnrolments.AddAsync(enrolment);

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}

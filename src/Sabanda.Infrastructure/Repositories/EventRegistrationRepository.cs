using Microsoft.EntityFrameworkCore;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;
using Sabanda.Infrastructure.Persistence;

namespace Sabanda.Infrastructure.Repositories;

public class EventRegistrationRepository : IEventRegistrationRepository
{
    private readonly SabandaDbContext _db;

    public EventRegistrationRepository(SabandaDbContext db)
    {
        _db = db;
    }

    public Task<EventRegistration?> FindByIdAsync(Guid id) =>
        _db.EventRegistrations.FirstOrDefaultAsync(r => r.Id == id);

    public Task<bool> IsDuplicateAsync(Guid eventId, Guid familyId, Guid? memberId) =>
        _db.EventRegistrations.AnyAsync(r =>
            r.EventId == eventId &&
            r.FamilyId == familyId &&
            r.Status != RegistrationStatus.Cancelled &&
            (memberId == null || r.MemberId == memberId));

    public Task<int> CountRegisteredAsync(Guid eventId) =>
        _db.EventRegistrations.CountAsync(r =>
            r.EventId == eventId &&
            r.Status == RegistrationStatus.Registered);

    public async Task<int> GetMaxWaitlistPositionAsync(Guid eventId)
    {
        var max = await _db.EventRegistrations
            .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
            .MaxAsync(r => (int?)r.WaitlistPosition);
        return max ?? 0;
    }

    public Task<EventRegistration?> GetFirstWaitlistedAsync(Guid eventId) =>
        _db.EventRegistrations
            .Where(r => r.EventId == eventId && r.Status == RegistrationStatus.Waitlisted)
            .OrderBy(r => r.WaitlistPosition)
            .FirstOrDefaultAsync();

    public Task<List<EventRegistration>> GetWaitlistedAfterPositionAsync(Guid eventId, int afterPosition) =>
        _db.EventRegistrations
            .Where(r =>
                r.EventId == eventId &&
                r.Status == RegistrationStatus.Waitlisted &&
                r.WaitlistPosition > afterPosition)
            .ToListAsync();

    public async Task AddAsync(EventRegistration registration) =>
        await _db.EventRegistrations.AddAsync(registration);

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}

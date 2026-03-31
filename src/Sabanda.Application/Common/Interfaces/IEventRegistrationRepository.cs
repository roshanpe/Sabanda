using Sabanda.Domain.Entities;

namespace Sabanda.Application.Common.Interfaces;

public interface IEventRegistrationRepository
{
    Task<EventRegistration?> FindByIdAsync(Guid id);
    Task<bool> IsDuplicateAsync(Guid eventId, Guid familyId, Guid? memberId);
    Task<int> CountRegisteredAsync(Guid eventId);
    Task<int> GetMaxWaitlistPositionAsync(Guid eventId);
    Task<EventRegistration?> GetFirstWaitlistedAsync(Guid eventId);
    Task<List<EventRegistration>> GetWaitlistedAfterPositionAsync(Guid eventId, int afterPosition);
    Task AddAsync(EventRegistration registration);
    Task SaveChangesAsync();
}

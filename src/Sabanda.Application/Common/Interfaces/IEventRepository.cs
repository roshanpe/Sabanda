using Sabanda.Domain.Entities;

namespace Sabanda.Application.Common.Interfaces;

public interface IEventRepository
{
    Task<Event?> FindByIdAsync(Guid id);
    Task AddAsync(Event @event);
    Task SaveChangesAsync();
}

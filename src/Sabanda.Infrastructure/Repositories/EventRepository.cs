using Microsoft.EntityFrameworkCore;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Entities;
using Sabanda.Infrastructure.Persistence;

namespace Sabanda.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly SabandaDbContext _db;

    public EventRepository(SabandaDbContext db)
    {
        _db = db;
    }

    public Task<Event?> FindByIdAsync(Guid id) =>
        _db.Events.FirstOrDefaultAsync(e => e.Id == id);

    public async Task AddAsync(Event @event) =>
        await _db.Events.AddAsync(@event);

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}

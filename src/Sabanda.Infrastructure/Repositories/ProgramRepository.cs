using Microsoft.EntityFrameworkCore;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Infrastructure.Persistence;
using Program = Sabanda.Domain.Entities.Program;

namespace Sabanda.Infrastructure.Repositories;

public class ProgramRepository : IProgramRepository
{
    private readonly SabandaDbContext _db;

    public ProgramRepository(SabandaDbContext db)
    {
        _db = db;
    }

    public Task<Program?> FindByIdAsync(Guid id) =>
        _db.Programs.FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(Program program) =>
        await _db.Programs.AddAsync(program);

    public Task SaveChangesAsync() =>
        _db.SaveChangesAsync();
}

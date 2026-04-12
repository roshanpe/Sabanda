using Microsoft.EntityFrameworkCore;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;

namespace Sabanda.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(SabandaDbContext db)
    {
        if (await db.Tenants.AnyAsync())
            return;

        var defaultTenant = new Tenant("Default Tenant", "default");
        await db.Tenants.AddAsync(defaultTenant);

        var devTenant = new Tenant("Development Tenant", "dev");
        await db.Tenants.AddAsync(devTenant);

        await db.SaveChangesAsync();

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234!", workFactor: 12);
        var defaultAdmin = new AppUser(defaultTenant.Id, "admin@sabanda.app", passwordHash, UserRole.Administrator);
        await db.AppUsers.AddAsync(defaultAdmin);

        var devAdmin = new AppUser(devTenant.Id, "admin@dev.com", passwordHash, UserRole.Administrator);
        await db.AppUsers.AddAsync(devAdmin);

        await db.SaveChangesAsync();
    }
}

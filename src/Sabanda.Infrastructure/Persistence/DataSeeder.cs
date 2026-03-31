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

        var tenant = new Tenant("Default Tenant", "default");
        await db.Tenants.AddAsync(tenant);
        await db.SaveChangesAsync();

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234!", workFactor: 12);
        var adminUser = new AppUser(tenant.Id, "admin@sabanda.app", passwordHash, UserRole.Administrator);
        await db.AppUsers.AddAsync(adminUser);
        await db.SaveChangesAsync();
    }
}

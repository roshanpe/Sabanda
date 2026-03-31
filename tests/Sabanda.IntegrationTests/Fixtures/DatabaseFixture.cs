using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;
using Sabanda.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Sabanda.IntegrationTests.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("sabanda_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public SabandaWebAppFactory Factory { get; private set; } = null!;
    public string ConnectionString => _postgres.GetConnectionString();

    // Seeded test data
    public Guid TenantId { get; private set; }
    public string TenantSlug { get; private set; } = "test-tenant";
    public Guid AdminUserId { get; private set; }
    public string AdminEmail { get; } = "admin@test.com";
    public string AdminPassword { get; } = "Admin@1234!";

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Factory = new SabandaWebAppFactory { ConnectionString = ConnectionString };

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SabandaDbContext>();
        await db.Database.MigrateAsync();

        // Seed test tenant
        var tenant = new Tenant("Test Tenant", TenantSlug);
        await db.Tenants.AddAsync(tenant);
        await db.SaveChangesAsync();
        TenantId = tenant.Id;

        // Seed admin user (bypass tenant filter for seeding)
        var hash = BCrypt.Net.BCrypt.HashPassword(AdminPassword, workFactor: 4); // low cost for tests
        var adminUser = new AppUser(TenantId, AdminEmail, hash, UserRole.Administrator);
        await db.AppUsers.AddAsync(adminUser);
        await db.SaveChangesAsync();
        AdminUserId = adminUser.Id;
    }

    public async Task DisposeAsync()
    {
        Factory.Dispose();
        await _postgres.DisposeAsync();
    }
}

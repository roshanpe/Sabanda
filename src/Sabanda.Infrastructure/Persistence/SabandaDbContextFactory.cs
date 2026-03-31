using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Sabanda.Application.Common.Interfaces;

namespace Sabanda.Infrastructure.Persistence;

/// <summary>Used by EF Core design-time tools (dotnet ef migrations add).</summary>
public class SabandaDbContextFactory : IDesignTimeDbContextFactory<SabandaDbContext>
{
    public SabandaDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("SABANDA_CONNECTION_STRING")
            ?? "Host=localhost;Database=sabanda_dev;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<SabandaDbContext>();
        optionsBuilder.UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new SabandaDbContext(optionsBuilder.Options, new NullTenantService());
    }

    private sealed class NullTenantService : ICurrentTenantService
    {
        public Guid TenantId => Guid.Empty;
        public string TenantSlug => string.Empty;
        public bool IsResolved => false;
        public void SetTenant(Guid tenantId, string slug) { }
    }
}

using Microsoft.EntityFrameworkCore;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Common;
using Sabanda.Domain.Entities;
using Program = Sabanda.Domain.Entities.Program;

namespace Sabanda.Infrastructure.Persistence;

public class SabandaDbContext : DbContext
{
    private readonly ICurrentTenantService _tenantService;

    public SabandaDbContext(DbContextOptions<SabandaDbContext> options, ICurrentTenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Family> Families => Set<Family>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<ActiveSession> ActiveSessions => Set<ActiveSession>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Program> Programs => Set<Program>();
    public DbSet<ProgramEnrolment> ProgramEnrolments => Set<ProgramEnrolment>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SabandaDbContext).Assembly);

        // Global tenant isolation filters — only active when tenant is resolved
        modelBuilder.Entity<Family>()
            .HasQueryFilter(e => !_tenantService.IsResolved || e.TenantId == _tenantService.TenantId);
        modelBuilder.Entity<Member>()
            .HasQueryFilter(e => !_tenantService.IsResolved || e.TenantId == _tenantService.TenantId);
        modelBuilder.Entity<AppUser>()
            .HasQueryFilter(e => !_tenantService.IsResolved || e.TenantId == _tenantService.TenantId);
        modelBuilder.Entity<Membership>()
            .HasQueryFilter(e => !_tenantService.IsResolved || e.TenantId == _tenantService.TenantId);
        modelBuilder.Entity<Program>()
            .HasQueryFilter(e => !_tenantService.IsResolved || e.TenantId == _tenantService.TenantId);
        modelBuilder.Entity<ProgramEnrolment>()
            .HasQueryFilter(e => !_tenantService.IsResolved || e.TenantId == _tenantService.TenantId);
        modelBuilder.Entity<Event>()
            .HasQueryFilter(e => !_tenantService.IsResolved || e.TenantId == _tenantService.TenantId);
        modelBuilder.Entity<EventRegistration>()
            .HasQueryFilter(e => !_tenantService.IsResolved || e.TenantId == _tenantService.TenantId);
        modelBuilder.Entity<AuditLog>()
            .HasQueryFilter(e => !_tenantService.IsResolved || e.TenantId == _tenantService.TenantId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_tenantService.IsResolved)
        {
            foreach (var entry in ChangeTracker.Entries<TenantScopedEntity>()
                         .Where(e => e.State == EntityState.Added))
            {
                entry.Entity.GetType()
                    .GetProperty(nameof(TenantScopedEntity.TenantId))!
                    .SetValue(entry.Entity, _tenantService.TenantId);
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

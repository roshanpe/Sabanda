using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Enums;
using Sabanda.Infrastructure.Persistence;
using Sabanda.Infrastructure.Services;
using Xunit;

namespace Sabanda.UnitTests.Infrastructure;

public class AuditLogServiceTests
{
    [Fact]
    public async Task LogAsync_AddsEntryWithoutCallingExplicitSave()
    {
        var tenantService = Substitute.For<ICurrentTenantService>();
        tenantService.IsResolved.Returns(true);
        tenantService.TenantId.Returns(Guid.NewGuid());

        var userService = Substitute.For<ICurrentUserService>();
        userService.IsAuthenticated.Returns(true);
        userService.UserId.Returns(Guid.NewGuid());

        var options = new DbContextOptionsBuilder<SabandaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var db = new SabandaDbContext(options, tenantService);

        var sut = new AuditLogService(db, tenantService, userService);

        await sut.LogAsync(AuditAction.Login, "AppUser", Guid.NewGuid(), new { test = "data" });

        // Should have staged the entry but NOT saved yet
        db.ChangeTracker.Entries().Should().HaveCount(1);
        var trackedEntry = db.ChangeTracker.Entries().Single();
        trackedEntry.State.Should().Be(EntityState.Added);

        // Now save explicitly
        await db.SaveChangesAsync();

        var logs = await db.AuditLogs.IgnoreQueryFilters().ToListAsync();
        logs.Should().HaveCount(1);
        logs[0].Action.Should().Be(AuditAction.Login);
    }
}

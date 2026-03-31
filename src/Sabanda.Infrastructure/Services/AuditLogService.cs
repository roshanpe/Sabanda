using System.Text.Json;
using Sabanda.Application.Common.Interfaces;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;
using Sabanda.Infrastructure.Persistence;

namespace Sabanda.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly SabandaDbContext _db;
    private readonly ICurrentTenantService _tenant;
    private readonly ICurrentUserService _currentUser;

    public AuditLogService(SabandaDbContext db, ICurrentTenantService tenant, ICurrentUserService currentUser)
    {
        _db = db;
        _tenant = tenant;
        _currentUser = currentUser;
    }

    // Does NOT call SaveChangesAsync - caller is responsible for committing.
    public Task LogAsync(AuditAction action, string? targetEntityType = null,
        Guid? targetEntityId = null, object? detail = null)
    {
        var detailJson = detail != null ? JsonSerializer.Serialize(detail) : null;
        var userId = _currentUser.IsAuthenticated ? _currentUser.UserId : (Guid?)null;

        var log = new AuditLog(
            _tenant.IsResolved ? _tenant.TenantId : Guid.Empty,
            userId,
            action,
            targetEntityType,
            targetEntityId,
            detailJson);

        _db.AuditLogs.Add(log);
        return Task.CompletedTask;
    }
}

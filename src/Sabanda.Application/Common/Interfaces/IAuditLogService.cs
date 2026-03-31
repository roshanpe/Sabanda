using Sabanda.Domain.Enums;

namespace Sabanda.Application.Common.Interfaces;

public interface IAuditLogService
{
    // Does NOT call SaveChangesAsync - the caller must commit.
    Task LogAsync(AuditAction action, string? targetEntityType = null,
        Guid? targetEntityId = null, object? detail = null);
}

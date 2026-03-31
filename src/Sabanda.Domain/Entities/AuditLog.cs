using Sabanda.Domain.Common;
using Sabanda.Domain.Enums;

namespace Sabanda.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid TenantId { get; }
    public Guid? UserId { get; }
    public AuditAction Action { get; }
    public string? TargetEntityType { get; }
    public Guid? TargetEntityId { get; }
    public string? DetailJson { get; }

    private AuditLog() { }

    public AuditLog(Guid tenantId, Guid? userId, AuditAction action,
        string? targetEntityType = null, Guid? targetEntityId = null, string? detailJson = null)
    {
        TenantId = tenantId;
        UserId = userId;
        Action = action;
        TargetEntityType = targetEntityType;
        TargetEntityId = targetEntityId;
        DetailJson = detailJson;
    }
}

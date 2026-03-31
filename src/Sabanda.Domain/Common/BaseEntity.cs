namespace Sabanda.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
}

public abstract class TenantScopedEntity : BaseEntity
{
    public Guid TenantId { get; protected set; }
}

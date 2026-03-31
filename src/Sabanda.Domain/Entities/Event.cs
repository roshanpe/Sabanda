using Sabanda.Domain.Common;
using Sabanda.Domain.Enums;

namespace Sabanda.Domain.Entities;

public class Event : TenantScopedEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTimeOffset EventDate { get; private set; }
    public int Capacity { get; private set; }
    public EventBillingType BillingType { get; private set; }
    public Guid? CoordinatorUserId { get; private set; }

    private Event() { }

    public Event(Guid tenantId, string name, DateTimeOffset eventDate, int capacity,
        EventBillingType billingType, string? description = null, Guid? coordinatorUserId = null)
    {
        TenantId = tenantId;
        Name = name;
        EventDate = eventDate;
        Capacity = capacity;
        BillingType = billingType;
        Description = description;
        CoordinatorUserId = coordinatorUserId;
    }
}

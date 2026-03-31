using Sabanda.Domain.Common;

namespace Sabanda.Domain.Entities;

public class Program : TenantScopedEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Capacity { get; private set; }
    public Guid? CoordinatorUserId { get; private set; }
    public string? ScheduleJson { get; private set; }

    private Program() { }

    public Program(Guid tenantId, string name, int capacity, string? description = null, Guid? coordinatorUserId = null)
    {
        TenantId = tenantId;
        Name = name;
        Capacity = capacity;
        Description = description;
        CoordinatorUserId = coordinatorUserId;
    }
}

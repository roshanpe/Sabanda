using Sabanda.Domain.Common;
using Sabanda.Domain.Enums;

namespace Sabanda.Domain.Entities;

public class Program : TenantScopedEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Capacity { get; private set; }
    public Guid? CoordinatorUserId { get; private set; }
    public string? AgeGroup { get; private set; }
    public Frequency? Frequency { get; private set; }
    public string? Venue { get; private set; }
    public DayOfWeek? Day { get; private set; }
    public TimeOnly? Time { get; private set; }
    public string? ScheduleJson { get; private set; }

    private Program() { }

    public Program(
        Guid tenantId,
        string name,
        int capacity,
        string? description = null,
        Guid? coordinatorUserId = null,
        string? ageGroup = null,
        Frequency? frequency = null,
        string? venue = null,
        DayOfWeek? day = null,
        TimeOnly? time = null)
    {
        TenantId = tenantId;
        Name = name;
        Capacity = capacity;
        Description = description;
        CoordinatorUserId = coordinatorUserId;
        AgeGroup = ageGroup;
        Frequency = frequency;
        Venue = venue;
        Day = day;
        Time = time;
    }
}

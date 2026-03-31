using Sabanda.Domain.Common;
using Sabanda.Domain.Enums;

namespace Sabanda.Domain.Entities;

public class EventRegistration : TenantScopedEntity
{
    public Guid EventId { get; private set; }
    public Guid FamilyId { get; private set; }
    public Guid? MemberId { get; private set; }
    public RegistrationStatus Status { get; private set; }
    public int? WaitlistPosition { get; private set; }
    public DateTimeOffset RegisteredAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    private EventRegistration() { }

    public EventRegistration(Guid tenantId, Guid eventId, Guid familyId, RegistrationStatus status,
        Guid? memberId = null, int? waitlistPosition = null)
    {
        TenantId = tenantId;
        EventId = eventId;
        FamilyId = familyId;
        MemberId = memberId;
        Status = status;
        WaitlistPosition = waitlistPosition;
        RegisteredAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        Status = RegistrationStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;
        WaitlistPosition = null;
    }

    public void Promote()
    {
        Status = RegistrationStatus.Registered;
        WaitlistPosition = null;
    }

    public void DecrementWaitlistPosition()
    {
        if (WaitlistPosition.HasValue && WaitlistPosition.Value > 1)
            WaitlistPosition--;
    }
}

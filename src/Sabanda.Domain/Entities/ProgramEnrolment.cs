using Sabanda.Domain.Common;
using Sabanda.Domain.Enums;

namespace Sabanda.Domain.Entities;

public class ProgramEnrolment : TenantScopedEntity
{
    public Guid ProgramId { get; private set; }
    public Guid MemberId { get; private set; }
    public EnrolmentStatus Status { get; private set; }
    public int? WaitlistPosition { get; private set; }
    public DateTimeOffset EnrolledAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    private ProgramEnrolment() { }

    public ProgramEnrolment(Guid tenantId, Guid programId, Guid memberId, EnrolmentStatus status, int? waitlistPosition = null)
    {
        TenantId = tenantId;
        ProgramId = programId;
        MemberId = memberId;
        Status = status;
        WaitlistPosition = waitlistPosition;
        EnrolledAt = DateTimeOffset.UtcNow;
    }

    public void Cancel()
    {
        Status = EnrolmentStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;
        WaitlistPosition = null;
    }

    public void Promote()
    {
        Status = EnrolmentStatus.Enrolled;
        WaitlistPosition = null;
    }

    public void DecrementWaitlistPosition()
    {
        if (WaitlistPosition.HasValue && WaitlistPosition.Value > 1)
            WaitlistPosition--;
    }
}

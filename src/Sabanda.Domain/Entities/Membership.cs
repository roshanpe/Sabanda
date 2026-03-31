using Sabanda.Domain.Common;
using Sabanda.Domain.Enums;

namespace Sabanda.Domain.Entities;

public class Membership : TenantScopedEntity
{
    public Guid FamilyId { get; private set; }
    public Guid? MemberId { get; private set; }
    public MembershipType Type { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }

    private static readonly Dictionary<PaymentStatus, HashSet<PaymentStatus>> ValidTransitions = new()
    {
        [PaymentStatus.Initiated] = [PaymentStatus.Pending, PaymentStatus.Failed],
        [PaymentStatus.Pending]   = [PaymentStatus.Completed, PaymentStatus.Failed],
        [PaymentStatus.Completed] = [PaymentStatus.Refunded],
        [PaymentStatus.Failed]    = [],
        [PaymentStatus.Refunded]  = [],
    };

    private Membership() { }

    public Membership(Guid tenantId, Guid familyId, MembershipType type,
        DateOnly startDate, DateOnly endDate, Guid? memberId = null)
    {
        TenantId = tenantId;
        FamilyId = familyId;
        MemberId = memberId;
        Type = type;
        StartDate = startDate;
        EndDate = endDate;
        PaymentStatus = PaymentStatus.Initiated;
    }

    public bool CanTransitionTo(PaymentStatus newStatus) =>
        ValidTransitions.TryGetValue(PaymentStatus, out var allowed) && allowed.Contains(newStatus);

    public void UpdatePaymentStatus(PaymentStatus newStatus)
    {
        if (!CanTransitionTo(newStatus))
            throw new InvalidOperationException(
                $"Cannot transition payment status from {PaymentStatus} to {newStatus}.");
        PaymentStatus = newStatus;
    }

    public bool IsActive(DateOnly today) =>
        PaymentStatus == PaymentStatus.Completed &&
        today >= StartDate &&
        today <= EndDate;
}

using FluentAssertions;
using Sabanda.Domain.Entities;
using Sabanda.Domain.Enums;
using Xunit;

namespace Sabanda.UnitTests.Domain;

public class MembershipTests
{
    private Membership MakeMembership(
        DateOnly start = default,
        DateOnly end = default,
        MembershipType type = MembershipType.Program)
    {
        var startDate = start == default ? new DateOnly(2026, 1, 1) : start;
        var endDate = end == default ? new DateOnly(2026, 12, 31) : end;
        return new Membership(Guid.NewGuid(), Guid.NewGuid(), type, startDate, endDate);
    }

    [Fact]
    public void NewMembership_HasInitiatedStatus()
    {
        var m = MakeMembership();
        m.PaymentStatus.Should().Be(PaymentStatus.Initiated);
    }

    [Theory]
    [InlineData(PaymentStatus.Initiated, PaymentStatus.Pending, true)]
    [InlineData(PaymentStatus.Initiated, PaymentStatus.Failed, true)]
    [InlineData(PaymentStatus.Pending, PaymentStatus.Completed, true)]
    [InlineData(PaymentStatus.Pending, PaymentStatus.Failed, true)]
    [InlineData(PaymentStatus.Completed, PaymentStatus.Refunded, true)]
    [InlineData(PaymentStatus.Initiated, PaymentStatus.Completed, false)]
    [InlineData(PaymentStatus.Initiated, PaymentStatus.Refunded, false)]
    [InlineData(PaymentStatus.Completed, PaymentStatus.Pending, false)]
    [InlineData(PaymentStatus.Failed, PaymentStatus.Completed, false)]
    [InlineData(PaymentStatus.Refunded, PaymentStatus.Completed, false)]
    public void PaymentStatusTransitions_AreValidatedCorrectly(
        PaymentStatus from, PaymentStatus to, bool expectedValid)
    {
        var m = MakeMembership();
        // Transition to the 'from' state first
        if (from == PaymentStatus.Pending)
            m.UpdatePaymentStatus(PaymentStatus.Pending);
        else if (from == PaymentStatus.Completed)
        {
            m.UpdatePaymentStatus(PaymentStatus.Pending);
            m.UpdatePaymentStatus(PaymentStatus.Completed);
        }
        else if (from == PaymentStatus.Failed)
            m.UpdatePaymentStatus(PaymentStatus.Failed);

        m.CanTransitionTo(to).Should().Be(expectedValid);
    }

    [Fact]
    public void UpdatePaymentStatus_InvalidTransition_Throws()
    {
        var m = MakeMembership();
        var act = () => m.UpdatePaymentStatus(PaymentStatus.Completed);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IsActive_ReturnsTrue_WhenCompletedAndInDateRange()
    {
        var m = MakeMembership(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));
        m.UpdatePaymentStatus(PaymentStatus.Pending);
        m.UpdatePaymentStatus(PaymentStatus.Completed);

        m.IsActive(new DateOnly(2026, 6, 15)).Should().BeTrue();
    }

    [Fact]
    public void IsActive_ReturnsFalse_WhenPending()
    {
        var m = MakeMembership();
        m.UpdatePaymentStatus(PaymentStatus.Pending);

        m.IsActive(new DateOnly(2026, 6, 15)).Should().BeFalse();
    }
}

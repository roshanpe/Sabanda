using FluentValidation;
using Sabanda.Application.Memberships.DTOs;

namespace Sabanda.Application.Memberships.Validators;

public class CreateMembershipRequestValidator : AbstractValidator<CreateMembershipRequest>
{
    public CreateMembershipRequestValidator()
    {
        RuleFor(x => x.FamilyId).NotEmpty();
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).NotEmpty().GreaterThan(x => x.StartDate)
            .WithMessage("EndDate must be after StartDate.");
    }
}

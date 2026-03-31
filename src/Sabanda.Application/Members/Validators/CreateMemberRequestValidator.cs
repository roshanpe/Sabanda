using FluentValidation;
using Sabanda.Application.Members.DTOs;

namespace Sabanda.Application.Members.Validators;

public class CreateMemberRequestValidator : AbstractValidator<CreateMemberRequest>
{
    public CreateMemberRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.Phone).MaximumLength(50).When(x => x.Phone != null);
        RuleFor(x => x.Gender).MaximumLength(50).When(x => x.Gender != null);

        // Consent is required for minors
        When(x => x.DateOfBirth.AddYears(18) > DateOnly.FromDateTime(DateTime.UtcNow), () =>
        {
            RuleFor(x => x.ConsentGiven)
                .Equal(true)
                .WithMessage("ConsentGiven must be true for minors.");
            RuleFor(x => x.ConsentGivenBy)
                .NotNull()
                .WithMessage("ConsentGivenBy is required for minors.");
            RuleFor(x => x.ConsentGivenAt)
                .NotNull()
                .WithMessage("ConsentGivenAt is required for minors.");
        });
    }
}

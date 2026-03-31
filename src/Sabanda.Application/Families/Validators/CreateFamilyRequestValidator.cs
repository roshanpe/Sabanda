using FluentValidation;
using Sabanda.Application.Families.DTOs;

namespace Sabanda.Application.Families.Validators;

public class CreateFamilyRequestValidator : AbstractValidator<CreateFamilyRequest>
{
    public CreateFamilyRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PrimaryHolderEmail).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PrimaryHolderPassword).NotEmpty().MinimumLength(8);
    }
}

using FluentValidation;
using Sabanda.Application.Programs.DTOs;

namespace Sabanda.Application.Programs.Validators;

public class CreateProgramRequestValidator : AbstractValidator<CreateProgramRequest>
{
    public CreateProgramRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacity).GreaterThan(0);
        RuleFor(x => x.AgeGroup).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.AgeGroup));
        RuleFor(x => x.Venue).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Venue));
        RuleFor(x => x.Frequency).IsInEnum().When(x => x.Frequency.HasValue);
    }
}

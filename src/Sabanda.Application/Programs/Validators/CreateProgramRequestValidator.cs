using FluentValidation;
using Sabanda.Application.Programs.DTOs;

namespace Sabanda.Application.Programs.Validators;

public class CreateProgramRequestValidator : AbstractValidator<CreateProgramRequest>
{
    public CreateProgramRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}

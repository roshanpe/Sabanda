using FluentValidation;
using Sabanda.Application.Events.DTOs;

namespace Sabanda.Application.Events.Validators;

public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EventDate).NotEmpty();
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}

using FluentValidation;
using EventReservationAPI.Entities;

namespace EventReservationAPI.Validators
{
    public class InputEventDtoValidator : AbstractValidator<InputEventDto>
    {
        public InputEventDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Event name is required.")
                .MaximumLength(100).WithMessage("Event name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Event description cannot exceed 500 characters.");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Event location is required.")
                .MaximumLength(200).WithMessage("Event location cannot exceed 200 characters.");

            RuleFor(x => x.StartsAt)
                .NotEmpty().WithMessage("Event start time is required.")
                .GreaterThan(DateTime.UtcNow).WithMessage("Event start time must be in the future.");

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Event capacity must be greater than zero.");
        }
    }
}

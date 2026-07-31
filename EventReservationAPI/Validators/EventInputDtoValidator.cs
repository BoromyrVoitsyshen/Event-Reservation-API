using FluentValidation;
using EventReservationAPI.Entities;

namespace EventReservationAPI.Validators
{
    public class EventInputDtoValidator : AbstractValidator<EventInputDto>
    {
        public const int MaxNameLength = 100;
        public const int MaxDescriptionLength = 500;
        public const int MaxLocationLength = 200;
        public EventInputDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Event name is required.")
                .MaximumLength(MaxNameLength).WithMessage($"Event name cannot exceed {MaxNameLength} characters.");

            RuleFor(x => x.Description)
                .MaximumLength(MaxDescriptionLength).WithMessage($"Event description cannot exceed {MaxDescriptionLength} characters.");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Event location is required.")
                .MaximumLength(MaxLocationLength).WithMessage($"Event location cannot exceed {MaxLocationLength} characters.");

            RuleFor(x => x.StartsAt)
                .NotEmpty().WithMessage("Event start time is required.")
                .GreaterThan(DateTime.UtcNow).WithMessage("Event start time must be in the future.");

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Event capacity must be greater than zero.");
        }
    }
}

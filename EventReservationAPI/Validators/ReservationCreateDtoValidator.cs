using FluentValidation;
using EventReservationAPI.Entities;

namespace EventReservationAPI.Validators
{
    public class ReservationCreateDtoValidator : AbstractValidator<ReservationCreateDto>
    {
        public ReservationCreateDtoValidator() 
        {
            RuleFor(x => x.EventId)
                        .GreaterThan(0).WithMessage("EventId is required.");

            RuleFor(x => x.SeatsCount)
                .GreaterThan(0).WithMessage("SeatsCount must be greater than zero.");
        }
    }
}

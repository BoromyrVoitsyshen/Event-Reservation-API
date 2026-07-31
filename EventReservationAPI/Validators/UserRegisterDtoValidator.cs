using FluentValidation;
using EventReservationAPI.Entities;

namespace EventReservationAPI.Validators
{
    public class UserRegisterDtoValidator : AbstractValidator<UserRegisterDto>
    {
        public const int MaxNameLength = 100;
        public const int MinPasswordLength = 6;
        public UserRegisterDtoValidator() 
        {
            RuleFor(x => x.Email)
                .NotNull().NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");
            RuleFor(x => x.Name)
                .NotNull().NotEmpty().WithMessage("Name is required.")
                .MaximumLength(MaxNameLength).WithMessage($"Name cannot exceed {MaxNameLength} characters.");
            RuleFor(x => x.Password)
                .NotNull().NotEmpty().WithMessage("Password is required.")
                .MinimumLength(MinPasswordLength).WithMessage($"Password must be at least {MinPasswordLength} characters long.");
        }
    }
}

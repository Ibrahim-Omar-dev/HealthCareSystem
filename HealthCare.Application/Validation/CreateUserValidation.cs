using HealthCare.Domain.User;
using FluentValidation;

namespace HealthCare.Application.Validation
{
    public class CreateUserValidation : AbstractValidator<CreateUser>
    {
        public CreateUserValidation() {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("FullName is required");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one number");


            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Password Not Match");
                
        }
    }
}

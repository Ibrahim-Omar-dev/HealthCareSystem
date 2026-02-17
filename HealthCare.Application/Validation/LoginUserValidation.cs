using HealthCare.Domain.User;
using FluentValidation;

namespace HealthCare.Application.Validation
{
    public class LoginUserValidation : AbstractValidator<LoginUser>
    {
        public LoginUserValidation()
        {

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }

    }
}

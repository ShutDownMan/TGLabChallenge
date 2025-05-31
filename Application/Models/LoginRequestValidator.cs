using FluentValidation;

namespace Application.Models
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty().WithMessage("Username or Email is required.")
                .WithErrorCode("USERNAME_OR_EMAIL_REQUIRED");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .WithErrorCode("PASSWORD_REQUIRED");
        }
    }
}

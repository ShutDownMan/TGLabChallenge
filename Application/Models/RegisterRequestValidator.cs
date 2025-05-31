using FluentValidation;
using System;

namespace Application.Models
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .WithErrorCode("USERNAME_REQUIRED");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .WithErrorCode("PASSWORD_REQUIRED");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .WithErrorCode("EMAIL_REQUIRED")
                .EmailAddress().WithMessage("Email format is invalid.")
                .WithErrorCode("EMAIL_INVALID");

            RuleFor(x => x.InitialBalance)
                .GreaterThanOrEqualTo(0).WithMessage("InitialBalance cannot be negative.")
                .WithErrorCode("INITIALBALANCE_NEGATIVE");
        }
    }
}

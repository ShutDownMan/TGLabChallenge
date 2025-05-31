using FluentValidation;

namespace Application.Models
{
    public class TokenRequestValidator : AbstractValidator<TokenRequest>
    {
        public TokenRequestValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token is required.")
                .WithErrorCode("TOKEN_REQUIRED");
        }
    }
}

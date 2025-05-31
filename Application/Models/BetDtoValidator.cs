using FluentValidation;

namespace Application.Models
{
    public class BetDtoValidator : AbstractValidator<BetDTO>
    {
        public BetDtoValidator()
        {
            // Check if amount is non-negative
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0.")
                .WithErrorCode("AMOUNT_GREATER_THAN_ZERO");

            RuleFor(x => x.PlayerId)
                .NotEmpty()
                .WithMessage("PlayerId is required.")
                .WithErrorCode("PLAYERID_REQUIRED");

            RuleFor(x => x.CurrencyId)
                .GreaterThan(0)
                .WithMessage("CurrencyId must be greater than 0.")
                .WithErrorCode("CURRENCYID_GREATER_THAN_ZERO");

            RuleFor(x => x.StatusId)
                .GreaterThan(0)
                .WithMessage("StatusId must be greater than 0.")
                .WithErrorCode("STATUSID_GREATER_THAN_ZERO");

            RuleFor(x => x.Prize)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Prize.HasValue)
                .WithMessage("Prize must be non-negative.")
                .WithErrorCode("PRIZE_NON_NEGATIVE");

            RuleFor(x => x.CreatedAt)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("CreatedAt must be in the past or present.")
                .WithErrorCode("CREATEDAT_PAST_OR_PRESENT");
        }
    }
}

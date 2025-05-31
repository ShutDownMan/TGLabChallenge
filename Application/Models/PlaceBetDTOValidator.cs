using FluentValidation;

namespace Application.Models
{
    public class PlaceBetDTOValidator : AbstractValidator<PlaceBetDTO>
    {
        public PlaceBetDTOValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0.")
                .WithErrorCode("AMOUNT_GREATER_THAN_ZERO");

            RuleFor(x => x.WalletId)
                .NotEmpty()
                .WithMessage("WalletId is required.")
                .WithErrorCode("WALLETID_REQUIRED");

            RuleFor(x => x.CurrencyId)
                .GreaterThan(0)
                .WithMessage("CurrencyId must be greater than 0.")
                .WithErrorCode("CURRENCYID_GREATER_THAN_ZERO");

            RuleFor(x => x.CreatedAt)
                .LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("CreatedAt must be in the past or present.")
                .WithErrorCode("CREATEDAT_PAST_OR_PRESENT");

            RuleFor(x => x.GameId)
                .NotEmpty()
                .WithMessage("GameId is required.")
                .WithErrorCode("GAMEID_REQUIRED");
        }
    }
}

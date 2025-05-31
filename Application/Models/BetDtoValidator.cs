using FluentValidation;

namespace Application.Models
{
    public class BetDtoValidator : AbstractValidator<BetDto>
    {
        public BetDtoValidator()
        {
            // Check if amount is non-negative
            RuleFor(x => x.Amount).GreaterThan(0);

            RuleFor(x => x.PlayerId).NotEmpty();
            RuleFor(x => x.CurrencyId).GreaterThan(0);
            RuleFor(x => x.StatusId).GreaterThan(0);
            RuleFor(x => x.Prize).GreaterThanOrEqualTo(0).When(x => x.Prize.HasValue);
            RuleFor(x => x.CreatedAt).LessThanOrEqualTo(DateTime.UtcNow).WithMessage("CreatedAt must be in the past or present.");
        }
    }
}

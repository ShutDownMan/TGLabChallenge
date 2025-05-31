using FluentValidation;

namespace Application.Models
{
    public class CancelBetDTOValidator : AbstractValidator<CancelBetDTO>
    {
        public CancelBetDTOValidator()
        {
            RuleFor(x => x.CancelReason)
                .MaximumLength(250)
                .WithMessage("Cancel reason must be 250 characters or fewer.")
                .WithErrorCode("CANCELREASON_TOO_LONG");
        }
    }
}

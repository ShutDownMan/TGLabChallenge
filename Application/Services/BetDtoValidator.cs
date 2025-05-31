using FluentValidation;
using Application.Models;
using System;

namespace Application.Services
{
    public class BetDTOValidator : AbstractValidator<Application.Models.BetDTO>
    {
        public BetDTOValidator()
        {
            RuleFor(x => x.PlayerId).NotEqual(Guid.Empty);
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.CurrencyId).GreaterThan(0);
            RuleFor(x => x.StatusId).GreaterThan(0);
        }
    }
}

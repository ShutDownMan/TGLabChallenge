using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Services
{
    /// <summary>
    /// Service responsible for handling currency-related operations.
    /// </summary>
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository _currencyRepository;

        public CurrencyService(ICurrencyRepository currencyRepository)
        {
            _currencyRepository = currencyRepository;
        }

        /// <summary>
        /// Retrieves the default currency asynchronously.
        /// </summary>
        /// <returns>The default currency.</returns>
        public async Task<Currency> GetDefaultCurrencyAsync()
        {
            return await _currencyRepository.GetDefaultCurrencyAsync();
        }

        /// <summary>
        /// Checks if a currency exists asynchronously.
        /// </summary>
        /// <param name="currencyId">The ID of the currency.</param>
        /// <returns>True if the currency exists; otherwise, false.</returns>
        public async Task<bool> CurrencyExistsAsync(int currencyId)
        {
            return await _currencyRepository.CurrencyExistsAsync(currencyId);
        }
    }
}

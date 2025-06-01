using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface ICurrencyService
    {
        Task<Currency> GetDefaultCurrencyAsync();
        Task<bool> CurrencyExistsAsync(int currencyId);
    }
}

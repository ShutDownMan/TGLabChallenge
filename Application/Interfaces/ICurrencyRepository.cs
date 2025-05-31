using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICurrencyRepository
    {
        Task<Currency> GetDefaultCurrencyAsync();
        Task<bool> CurrencyExistsAsync(int currencyId);
    }
}

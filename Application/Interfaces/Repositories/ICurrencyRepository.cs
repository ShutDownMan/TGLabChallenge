using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface ICurrencyRepository
    {
        Task<Currency> GetDefaultCurrencyAsync();
        Task<bool> CurrencyExistsAsync(int currencyId);
    }
}
using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly AppDbContext _context;

        public CurrencyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Currency> GetDefaultCurrencyAsync()
        {
            return await _context.Currencies.OrderBy(c => c.Id).FirstAsync();
        }

        public async Task<bool> CurrencyExistsAsync(int currencyId)
        {
            return await _context.Currencies.AnyAsync(c => c.Id == currencyId);
        }
    }
}

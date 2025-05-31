using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IWalletRepository
    {
        Task AddAsync(Wallet wallet);
        // Add other methods as needed, e.g.:
        // Task<Wallet> GetByIdAsync(Guid id);
        // Task UpdateAsync(Wallet wallet);
    }
}

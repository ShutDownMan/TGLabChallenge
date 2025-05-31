using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IWalletTransactionRepository
    {
        Task<WalletTransaction> GetByIdAsync(Guid id);
        Task AddAsync(WalletTransaction transaction);
        Task<List<WalletTransaction>> GetByWalletIdAsync(Guid walletId); // Added method
    }
}
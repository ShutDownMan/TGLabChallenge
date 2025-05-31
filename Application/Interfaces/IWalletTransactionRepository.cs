using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IWalletTransactionRepository
    {
        Task<WalletTransaction> GetByIdAsync(Guid id);
        Task AddAsync(WalletTransaction transaction);
    }
}

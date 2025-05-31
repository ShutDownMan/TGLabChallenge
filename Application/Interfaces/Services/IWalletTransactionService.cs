using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Models;

namespace Application.Interfaces.Services
{
    public interface IWalletTransactionService
    {
        Task AddAsync(WalletTransaction transaction);
        Task<IEnumerable<WalletTransaction>> GetByWalletIdAsync(Guid walletId);

        Task<IEnumerable<WalletTransactionDTO>> GetTransactionInfosByWalletIdAsync(Guid walletId);

        Task<WalletTransaction> DebitWalletAsync(Wallet wallet, decimal amount, Guid? betId = null);
        Task<WalletTransaction> CreditWalletAsync(Wallet wallet, decimal amount, Guid? betId = null);
        Task<WalletTransaction> CheckpointWalletAsync(Wallet wallet, decimal checkpointAmount);
    }
}

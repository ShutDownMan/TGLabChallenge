using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IWalletService
    {
        Task<List<Wallet>> GetWalletsByPlayerIdAsync(Guid playerId);
        Task<Guid?> GetPlayerByWalletIdAsync(Guid walletId);
        Task<Wallet?> GetWalletByIdAsync(Guid walletId);
        Task<bool> WalletExistsAsync(Guid walletId);
        Task UpdateWalletAsync(Wallet wallet);

        Task<WalletTransaction> DebitWalletAsync(Wallet wallet, decimal amount, Guid? betId = null);
        Task<WalletTransaction> CreditWalletAsync(Wallet wallet, decimal amount, Guid? betId = null);
        Task<WalletTransaction> CalculateWalletCheckpointAsync(Wallet wallet);
        Task<Currency?> GetCurrencyByWalletIdAsync(Guid walletId);
        Task<IEnumerable<Bet>> GetBetsByWalletIdAsync(Guid walletId);
    }
}

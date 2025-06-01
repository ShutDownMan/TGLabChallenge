using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IWalletRepository
    {
        Task AddAsync(Wallet wallet);
        Task<List<Wallet>> GetWalletsByPlayerIdAsync(Guid playerId);
        Task UpdateAsync(Wallet wallet);
        Task<Guid?> GetPlayerByWalletIdAsync(Guid walletId);
        Task<Wallet?> GetByIdAsync(Guid walletId);
        Task<IEnumerable<Bet>> GetBetsByWalletIdAsync(Guid walletId);
    }
}

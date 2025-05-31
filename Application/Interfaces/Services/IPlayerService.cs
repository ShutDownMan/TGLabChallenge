using System.Threading.Tasks;
using Application.Models;
using System;
using System.Collections.Generic;

namespace Application.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<PlayerProfileDTO?> GetProfileAsync(Guid playerId);
        Task<IEnumerable<BetDTO>> GetBetsAsync(Guid playerId);
        Task<IEnumerable<WalletTransactionDTO>> GetWalletTransactionsAsync(Guid playerId);
    }
}
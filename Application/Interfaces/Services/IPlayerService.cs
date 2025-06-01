using System.Threading.Tasks;
using Application.Models;
using System;
using System.Collections.Generic;
using Domain.Entities;

namespace Application.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<PlayerProfileDTO?> GetProfileAsync(Guid playerId);
        Task<IEnumerable<BetDTO>> GetBetsAsync(Guid playerId, int pageNumber, int pageSize);
        Task<IEnumerable<WalletTransactionDTO>> GetWalletTransactionsAsync(Guid playerId, int pageNumber, int pageSize);
        Task<Player?> GetByUsernameAsync(string username);
        Task<Player?> GetByEmailAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task AddAsync(Player player);
    }
}
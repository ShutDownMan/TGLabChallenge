using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;

        public WalletService(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<List<Wallet>> GetWalletsByPlayerIdAsync(Guid playerId)
        {
            return await _walletRepository.GetWalletsByPlayerIdAsync(playerId);
        }
    }
}

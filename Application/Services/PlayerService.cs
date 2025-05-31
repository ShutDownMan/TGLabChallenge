using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IWalletService _walletService;
        private readonly IBetService _betService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly ILogger<PlayerService> _logger;

        public PlayerService(
            IPlayerRepository playerRepository,
            IWalletService walletService,
            IBetService betService,
            IWalletTransactionService walletTransactionService,
            ILogger<PlayerService> logger)
        {
            _playerRepository = playerRepository;
            _walletService = walletService;
            _betService = betService;
            _walletTransactionService = walletTransactionService;
            _logger = logger;
        }

        public async Task<PlayerProfileDTO?> GetProfileAsync(Guid playerId)
        {
            _logger.LogInformation("Starting GetProfileAsync for playerId: {PlayerId}", playerId);

            var player = await _playerRepository.GetByIdAsync(playerId);
            if (player == null)
            {
                _logger.LogWarning("Player with ID {PlayerId} not found.", playerId);
                return null;
            }

            _logger.LogInformation("Player found: {Username} (ID: {PlayerId})", player.Username, player.Id);

            var wallets = await _walletService.GetWalletsByPlayerIdAsync(playerId);
            if (wallets == null || wallets.Count == 0)
            {
                _logger.LogWarning("No wallets found for player with ID {PlayerId}.", playerId);
            }
            else
            {
                _logger.LogInformation("{WalletCount} wallet(s) found for player with ID {PlayerId}.", wallets.Count, playerId);
            }

            var profile = new PlayerProfileDTO
            {
                Id = player.Id,
                Username = player.Username,
                Email = player.Email,
                CreatedAt = player.CreatedAt,
                Wallets = wallets!.ConvertAll(wallet => new WalletDTO
                {
                    Id = wallet.Id,
                    CurrencyId = wallet.CurrencyId,
                    Balance = wallet.Balance,
                    CreatedAt = wallet.CreatedAt,
                    Currency = wallet.Currency == null ? null : new CurrencyDTO
                    {
                        Id = wallet.Currency.Id,
                        Code = wallet.Currency.Code,
                        Name = wallet.Currency.Name
                    }
                })
            };

            _logger.LogInformation("Returning PlayerProfileDTO for playerId: {PlayerId}", playerId);
            return profile;
        }

        public async Task<IEnumerable<BetDTO>> GetBetsAsync(Guid playerId)
        {
            // You may want to validate the player exists, or just delegate to the bet service
            return await _betService.GetBetsByUserAsync(playerId);
        }

        public async Task<IEnumerable<WalletTransactionDTO>> GetWalletTransactionsAsync(Guid playerId)
        {
            var wallets = await _walletService.GetWalletsByPlayerIdAsync(playerId);
            var allTransactions = new List<WalletTransactionDTO>();
            foreach (var wallet in wallets)
            {
                var transactions = await _walletTransactionService.GetTransactionInfosByWalletIdAsync(wallet.Id);
                allTransactions.AddRange(transactions);
            }

            return allTransactions;
        }
    }
}

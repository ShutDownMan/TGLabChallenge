using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    /// <summary>
    /// Service responsible for handling player-related operations.
    /// </summary>
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IWalletService _walletService;
        private readonly IBetService _betService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly ILogger<PlayerService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerService"/> class.
        /// </summary>
        /// <param name="playerRepository">The repository for accessing player data.</param>
        /// <param name="walletService">The service for handling wallet operations.</param>
        /// <param name="betService">The service for handling bet operations.</param>
        /// <param name="walletTransactionService">The service for handling wallet transaction operations.</param>
        /// <param name="logger">The logger instance for logging.</param>
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

        /// <summary>
        /// Retrieves the profile of a player by their unique identifier.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>A <see cref="PlayerProfileDTO"/> containing the player's profile information, or null if the player is not found.</returns>
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

        /// <summary>
        /// Retrieves all bets made by a player.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>A collection of <see cref="BetDTO"/> representing the player's bets.</returns>
        public async Task<IEnumerable<BetDTO>> GetBetsAsync(Guid playerId)
        {
            // You may want to validate the player exists, or just delegate to the bet service
            return await _betService.GetBetsByUserAsync(playerId);
        }

        /// <summary>
        /// Retrieves all wallet transactions for a player.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <returns>A collection of <see cref="WalletTransactionDTO"/> representing the player's wallet transactions.</returns>
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

        public async Task<Player?> GetByUsernameAsync(string username)
        {
            return await _playerRepository.GetByUsernameAsync(username);
        }

        public async Task<Player?> GetByEmailAsync(string email)
        {
            return await _playerRepository.GetByEmailAsync(email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _playerRepository.UsernameExistsAsync(username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _playerRepository.EmailExistsAsync(email);
        }

        public async Task AddAsync(Player player)
        {
            await _playerRepository.AddAsync(player);
        }
    }
}

using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

using TransactionTypeEnum = Domain.Enums.TransactionType;
using BetStatusEnum = Domain.Enums.BetStatus;

namespace Application.Services
{
    public class BetService : IBetService
    {
        private readonly IBetRepository _betRepository;
        private readonly IMapper _mapper;
        private readonly IWalletService _walletService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IGameService _gameService;
        private readonly ILogger<BetService> _logger;
        private readonly IUserNotificationService _userNotificationService;
        private readonly IRandomService _randomService;

        public BetService(
            IBetRepository betRepository,
            IMapper mapper,
            IWalletService walletService,
            IWalletTransactionService walletTransactionService,
            IGameService gameService,
            ILogger<BetService> logger,
            IUserNotificationService userNotificationService,
            IRandomService randomService)
        {
            _betRepository = betRepository;
            _mapper = mapper;
            _walletService = walletService;
            _walletTransactionService = walletTransactionService;
            _gameService = gameService;
            _logger = logger;
            _userNotificationService = userNotificationService;
            _randomService = randomService;
        }

        public async Task<BetDTO> PlaceBetAsync(PlaceBetDTO betDTO)
        {
            _logger.LogDebug("Starting PlaceBetAsync for WalletId: {WalletId}, Amount: {Amount}", betDTO.WalletId, betDTO.Amount);

            var wallet = await ValidateAndGetWalletAsync(betDTO);
            var game = await ValidateAndGetGameAsync(betDTO);

            ValidateBetAmountAndCurrency(betDTO, game);
            ValidateWalletBalance(wallet, betDTO.Amount);

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var betEntity = CreateBetEntity(betDTO);

                var newBetEntity = await _betRepository.AddAsync(betEntity);
                if (newBetEntity == null)
                {
                    _logger.LogError("Failed to add bet. WalletId: {WalletId}, Amount: {Amount}", betDTO.WalletId, betDTO.Amount);
                    throw new InvalidOperationException("Failed to place bet.");
                }

                _logger.LogDebug("Bet added. BetId: {BetId}, WalletId: {WalletId}", betEntity.Id, wallet.Id);

                await _walletService.DebitWalletAsync(wallet, betDTO.Amount, newBetEntity.Id);

                _logger.LogDebug("Debited wallet. WalletId: {WalletId}, NewBalance: {Balance}", wallet.Id, wallet.Balance);

                var refreshedWallet = await _walletService.GetWalletByIdAsync(wallet.Id);
                if (refreshedWallet == null)
                {
                    _logger.LogWarning("Refreshed wallet not found. WalletId: {WalletId}", wallet.Id);
                    throw new InvalidOperationException("Refreshed wallet not found.");
                }

                _logger.LogDebug("Fetched refreshed wallet. WalletId: {WalletId}, Balance: {Balance}", refreshedWallet.Id, refreshedWallet.Balance);

                _logger.LogInformation("PlaceBetAsync completed successfully for BetId: {BetId}", betEntity.Id);

                var betDto = new BetDTO
                {
                    Id = newBetEntity.Id,
                    WalletId = newBetEntity.WalletId,
                    Amount = newBetEntity.Amount,
                    StatusId = newBetEntity.StatusId,
                    Status = ((BetStatusEnum)newBetEntity.StatusId).ToString(),
                    Payout = newBetEntity.StatusId == (int)BetStatusEnum.Settled ? newBetEntity.Payout : null,
                    CreatedAt = newBetEntity.CreatedAt,
                    GameId = newBetEntity.GameId,
                    Note = newBetEntity.Note,
                    LastUpdatedAt = newBetEntity.LastUpdatedAt
                };

                // Notify the user about the updated bet
                var playerId = await _walletService.GetPlayerByWalletIdAsync(betDTO.WalletId);
                if (playerId != null)
                {
                    await _userNotificationService.NotifyBetUpdateAsync(playerId.Value, betDto);
                }

                scope.Complete();

                return betDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in PlaceBetAsync for WalletId: {WalletId}", betDTO.WalletId);
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }

        private async Task<Wallet> ValidateAndGetWalletAsync(PlaceBetDTO betDTO)
        {
            var walletExists = await _walletService.WalletExistsAsync(betDTO.WalletId);
            if (!walletExists)
            {
                _logger.LogWarning("Wallet does not exist. WalletId: {WalletId}", betDTO.WalletId);
                throw new InvalidOperationException("Wallet does not exist.");
            }

            var playerId = await _walletService.GetPlayerByWalletIdAsync(betDTO.WalletId);
            if (playerId == null)
            {
                _logger.LogWarning("Player not found for WalletId: {WalletId}", betDTO.WalletId);
                throw new InvalidOperationException("Player not found.");
            }

            var wallets = await _walletService.GetWalletsByPlayerIdAsync(playerId.Value);
            var wallet = wallets.FirstOrDefault(w => w.Id == betDTO.WalletId);

            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found. WalletId: {WalletId}", betDTO.WalletId);
                throw new InvalidOperationException("Wallet not found.");
            }

            return wallet;
        }

        private async Task<Game> ValidateAndGetGameAsync(PlaceBetDTO betDTO)
        {
            var game = await _gameService.GetGameByIdAsync(betDTO.GameId);
            if (game == null)
            {
                _logger.LogWarning("Game not found. GameId: {GameId}", betDTO.GameId);
                throw new InvalidOperationException("Game not found.");
            }
            return game;
        }

        private void ValidateBetAmountAndCurrency(PlaceBetDTO betDTO, Game game)
        {
            if (betDTO.Amount < game.MinimalBetAmount)
            {
                _logger.LogWarning("Bet amount below minimum. GameId: {GameId}, MinimalBetAmount: {MinimalBetAmount}, BetAmount: {BetAmount}", betDTO.GameId, game.MinimalBetAmount, betDTO.Amount);
                throw new InvalidOperationException($"Bet amount must be at least {game.MinimalBetAmount}.");
            }
            if (betDTO.CurrencyId != game.MinimalBetCurrencyId)
            {
                _logger.LogWarning("Bet currency does not match game's minimal bet currency. GameId: {GameId}, GameCurrencyId: {GameCurrencyId}, BetCurrencyId: {BetCurrencyId}", betDTO.GameId, game.MinimalBetCurrencyId, betDTO.CurrencyId);
                throw new InvalidOperationException("Bet currency does not match game's minimal bet currency.");
            }
        }

        private void ValidateWalletBalance(Domain.Entities.Wallet wallet, decimal amount)
        {
            if (wallet.Balance < amount)
            {
                _logger.LogWarning("Insufficient wallet balance. WalletId: {WalletId}, Balance: {Balance}, BetAmount: {Amount}", wallet.Id, wallet.Balance, amount);
                throw new InvalidOperationException("Insufficient wallet balance.");
            }
        }

        private Bet CreateBetEntity(PlaceBetDTO betDTO)
        {
            var betEntity = _mapper.Map<Bet>(betDTO);
            betEntity.StatusId = (int)BetStatusEnum.Created;
            return betEntity;
        }

        public async Task<IEnumerable<BetDTO>> GetBetsByUserAsync(Guid userId)
        {
            _logger.LogDebug("Starting GetBetsByUserAsync for UserId: {UserId}", userId);

            var bets = await _betRepository.GetByUserAsync(userId);

            _logger.LogDebug("Found {BetCount} bets for UserId: {UserId}", bets.Count(), userId);

            var result = bets.Select(bet => new BetDTO
            {
                Id = bet.Id,
                WalletId = bet.WalletId,
                Amount = bet.Amount,
                StatusId = bet.StatusId,
                Status = ((BetStatusEnum)bet.StatusId).ToString(),
                Payout = bet.StatusId == (int)BetStatusEnum.Settled ? bet.Payout : null,
                CreatedAt = bet.CreatedAt,
                GameId = bet.GameId,
                Note = bet.Note,
                LastUpdatedAt = bet.LastUpdatedAt
            });

            _logger.LogDebug("Completed GetBetsByUserAsync for UserId: {UserId}", userId);

            return result;
        }

        public async Task CancelBetAsync(Guid betId, string? cancelReason)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var bet = await _betRepository.GetByIdAsync(betId);
                if (bet == null)
                    throw new KeyNotFoundException("Bet not found.");

                if (bet.StatusId != (int)BetStatusEnum.Created)
                    throw new InvalidOperationException($"Cannot cancel a bet with status '{((BetStatusEnum)bet.StatusId).ToString()}'.");

                // Fetch game using GameService
                var game = await _gameService.GetGameByIdAsync(bet.GameId);
                if (game == null)
                    throw new InvalidOperationException("Game not found for bet.");

                var taxPercentage = game.CancelTaxPercentage;
                var taxAmount = Math.Round(bet.Amount * taxPercentage / 100m, 2);
                var refundAmount = bet.Amount - taxAmount;

                // Refund the wallet (credit)
                var wallet = await _walletService.GetWalletByIdAsync(bet.WalletId);
                if (wallet == null)
                    throw new InvalidOperationException("Wallet not found for bet.");

                // No need to credit if refund amount is zero
                if (refundAmount > 0)
                {
                    await _walletService.CreditWalletAsync(wallet, refundAmount, bet.Id);
                }

                bet.StatusId = (int)BetStatusEnum.Cancelled;
                bet.Note = cancelReason;
                bet.LastUpdatedAt = DateTime.UtcNow;
                _logger.LogDebug("Cancelling bet with ID: {BetId}, Reason: {CancelReason}, Tax: {TaxAmount}, Refund: {RefundAmount}", betId, cancelReason, taxAmount, refundAmount);

                await _betRepository.UpdateAsync(bet);

                // Notify the user about the cancelled bet
                var playerId = await _walletService.GetPlayerByWalletIdAsync(bet.WalletId);
                if (playerId != null)
                {
                    var betDto = _mapper.Map<BetDTO>(bet);
                    await _userNotificationService.NotifyBetCancelledAsync(playerId.Value, betDto);
                }

                scope.Complete();
            }
            finally
            {
                scope.Dispose();
            }
        }

        public async Task<BetDTO?> GetBetByIdAsync(Guid id)
        {
            var bet = await _betRepository.GetByIdAsync(id);

            return bet == null ? null : _mapper.Map<BetDTO>(bet);
        }

        public async Task<BetDTO> SettleBetAsync(Guid betId)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var bet = await _betRepository.GetByIdAsync(betId);
                if (bet == null)
                    throw new KeyNotFoundException("Bet not found.");

                if (bet.StatusId != (int)BetStatusEnum.Created)
                    throw new InvalidOperationException($"Cannot settle a bet with status '{((BetStatusEnum)bet.StatusId).ToString()}'.");

                var game = await _gameService.GetGameByIdAsync(bet.GameId);
                if (game == null)
                    throw new InvalidOperationException("Game not found for bet.");

                bool playerWon = DeterminePlayerWin();
                if (!playerWon)
                {
                    bet.StatusId = (int)BetStatusEnum.Settled;
                    bet.Payout = 0;
                    bet.LastUpdatedAt = DateTime.UtcNow;
                    bet.Note = "Bet settled: player lost, no payout.";
                    _logger.LogDebug("Settling bet with ID: {BetId}, Player lost, no payout.", betId);
                    await _betRepository.UpdateAsync(bet);

                    // Notify the user about the settled bet
                    var playerId = await _walletService.GetPlayerByWalletIdAsync(bet.WalletId);
                    if (playerId != null)
                    {
                        var betDto = _mapper.Map<BetDTO>(bet);
                        await _userNotificationService.NotifyBetSettledAsync(playerId.Value, betDto);
                    }

                    scope.Complete();
                    return _mapper.Map<BetDTO>(bet);
                }

                // For demonstration, let's assume payout is double the bet amount
                var payout = bet.Amount * 2;

                // Credit the wallet with payout
                var wallet = await _walletService.GetWalletByIdAsync(bet.WalletId);
                if (wallet == null)
                    throw new InvalidOperationException("Wallet not found for bet.");

                await _walletService.CreditWalletAsync(wallet, payout, bet.Id);

                bet.StatusId = (int)BetStatusEnum.Settled;
                bet.Payout = payout;
                bet.LastUpdatedAt = DateTime.UtcNow;
                var currency = await _walletService.GetCurrencyByWalletIdAsync(bet.WalletId);
                if (currency == null)
                    throw new InvalidOperationException("Currency not found for wallet.");

                bet.Note = $"Bet settled: player won, payout {payout.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} {currency.Code}.";

                _logger.LogDebug("Settling bet with ID: {BetId}, Payout: {Payout}", betId, payout);

                await _betRepository.UpdateAsync(bet);

                // Notify the user about the settled bet
                var playerIdWin = await _walletService.GetPlayerByWalletIdAsync(bet.WalletId);
                if (playerIdWin != null)
                {
                    var betDto = _mapper.Map<BetDTO>(bet);
                    await _userNotificationService.NotifyBetSettledAsync(playerIdWin.Value, betDto);
                }

                scope.Complete();

                return _mapper.Map<BetDTO>(bet);
            }
            finally
            {
                scope.Dispose();
            }
        }

        // Dummy logic: player wins if Randomly generated number is even
        private bool DeterminePlayerWin()
        {
            return _randomService.GetRandomBoolean();
        }
    }
}

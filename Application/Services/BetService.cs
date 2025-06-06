﻿using Application.Exceptions;
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
    /// <summary>
    /// Service responsible for handling betting-related operations.
    /// </summary>
    public class BetService : IBetService
    {
        private readonly IBetRepository _betRepository;
        private readonly IMapper _mapper;
        private readonly IWalletService _walletService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IGameService _gameService;
        private readonly IUserNotificationService _userNotificationService;
        private readonly IRandomService _randomService;
        private readonly ILogger<BetService> _logger;

        public BetService(
            IBetRepository betRepository,
            IMapper mapper,
            IWalletService walletService,
            IWalletTransactionService walletTransactionService,
            IGameService gameService,
            IUserNotificationService userNotificationService,
            IRandomService randomService,
            ILogger<BetService> logger)
        {
            _betRepository = betRepository;
            _mapper = mapper;
            _walletService = walletService;
            _walletTransactionService = walletTransactionService;
            _gameService = gameService;
            _userNotificationService = userNotificationService;
            _randomService = randomService;
            _logger = logger;
        }

        /// <summary>
        /// Places a bet for a given wallet and game.
        /// </summary>
        /// <param name="betDTO">The details of the bet to be placed.</param>
        /// <returns>The placed bet details.</returns>
        /// <exception cref="BetValidationException">Thrown if the bet amount is below the game's minimum requirement.</exception>
        /// <exception cref="InvalidOperationException">Thrown if validation fails or the bet cannot be placed.</exception>
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

                _logger.LogDebug(
                    "Fetched refreshed wallet. WalletId: {WalletId}, Balance: {Balance}",
                    refreshedWallet.Id,
                    refreshedWallet.Balance
                );

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

        /// <summary>
        /// Retrieves all bets placed by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A collection of bets placed by the user.</returns>
        public async Task<IEnumerable<BetDTO>> GetBetsByUserAsync(Guid userId, int pageNumber, int pageSize)
        {
            _logger.LogDebug("Starting GetBetsByUserAsync for UserId: {UserId}", userId);

            var bets = await _betRepository.GetByUserAsync(userId);

            _logger.LogDebug("Found {BetCount} bets for UserId: {UserId}", bets.Count(), userId);

            var paginatedBets = bets.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var result = paginatedBets.Select(bet => new BetDTO
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

        /// <summary>
        /// Cancels a bet with the given ID and optional reason.
        /// </summary>
        /// <param name="betId">The ID of the bet to cancel.</param>
        /// <param name="cancelReason">The reason for cancellation (optional).</param>
        /// <exception cref="KeyNotFoundException">Thrown if the bet is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the bet cannot be canceled.</exception>
        public async Task CancelBetAsync(Guid betId, string? cancelReason)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var bet = await _betRepository.GetByIdAsync(betId);
                if (bet == null)
                    throw new KeyNotFoundException("Bet not found.");

                if (bet.StatusId != (int)BetStatusEnum.Created)
                    throw new InvalidOperationException(
                        $"Cannot cancel a bet with status '{((BetStatusEnum)bet.StatusId).ToString()}'."
                    );

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
                _logger.LogDebug(
                    "Cancelling bet with ID: {BetId}, Reason: {CancelReason}, Tax: {TaxAmount}, Refund: {RefundAmount}",
                    betId,
                    cancelReason,
                    taxAmount,
                    refundAmount
                );

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

        /// <summary>
        /// Retrieves the details of a bet by its ID.
        /// </summary>
        /// <param name="id">The ID of the bet.</param>
        /// <returns>The bet details, or null if not found.</returns>
        public async Task<BetDTO?> GetBetByIdAsync(Guid id)
        {
            var bet = await _betRepository.GetByIdAsync(id);
            return bet == null ? null : _mapper.Map<BetDTO>(bet);
        }

        /// <summary>
        /// Settles a bet with the given ID, determining whether the player wins or loses.
        /// </summary>
        /// <param name="betId">The ID of the bet to settle.</param>
        /// <returns>The settled bet details.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the bet is not found.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the bet cannot be settled.</exception>
        public async Task<BetDTO> SettleBetAsync(Guid betId)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var bet = await _betRepository.GetByIdAsync(betId);
                if (bet == null)
                    throw new KeyNotFoundException("Bet not found.");

                if (bet.StatusId != (int)BetStatusEnum.Created)
                    throw new InvalidOperationException(
                        $"Cannot settle a bet with status '{((BetStatusEnum)bet.StatusId).ToString()}'."
                    );

                var game = await _gameService.GetGameByIdAsync(bet.GameId);
                if (game == null)
                    throw new InvalidOperationException("Game not found for bet.");

                bool playerWon = DeterminePlayerWin();
                bet.IsWon = playerWon;

                if (!playerWon)
                {
                    bet.StatusId = (int)BetStatusEnum.Settled;
                    bet.Payout = 0;
                    bet.LastUpdatedAt = DateTime.UtcNow;
                    bet.Note = "Bet settled: player lost, no payout.";
                    _logger.LogDebug("Settling bet with ID: {BetId}, Player lost, no payout.", betId);
                    await _betRepository.UpdateAsync(bet);

                    await TrackLostBetAsync(bet.WalletId, bet.Amount, bet.GameId);

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

                // Calculate payout based on game's Odds attribute
                var payout = bet.Amount / (1 / game.Odds);

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

                bet.Note = string.Format(
                    "Bet settled: player won, payout {0} {1}.",
                    payout.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                    currency.Code
                );

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

        /// <summary>
        /// Validates and retrieves the wallet associated with a bet.
        /// </summary>
        /// <param name="betDTO">The bet details.</param>
        /// <returns>The validated wallet.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the wallet or player is not found.</exception>
        private async Task<Wallet> ValidateAndGetWalletAsync(PlaceBetDTO betDTO)
        {
            var walletExists = await _walletService.WalletExistsAsync(betDTO.WalletId);
            if (!walletExists)
                throw new WalletNotFoundException($"Wallet with ID {betDTO.WalletId} does not exist.");

            var playerId = await _walletService.GetPlayerByWalletIdAsync(betDTO.WalletId);
            if (playerId == null)
                throw new PlayerNotFoundException($"Player not found for Wallet ID {betDTO.WalletId}.");

            var wallets = await _walletService.GetWalletsByPlayerIdAsync(playerId.Value);
            var wallet = wallets.FirstOrDefault(w => w.Id == betDTO.WalletId);

            if (wallet == null)
                throw new WalletNotFoundException($"Wallet with ID {betDTO.WalletId} not found.");

            return wallet;
        }

        /// <summary>
        /// Validates and retrieves the game associated with a bet.
        /// </summary>
        /// <param name="betDTO">The bet details.</param>
        /// <returns>The validated game.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the game is not found.</exception>
        private async Task<Game> ValidateAndGetGameAsync(PlaceBetDTO betDTO)
        {
            var game = await _gameService.GetGameByIdAsync(betDTO.GameId);
            if (game == null)
                throw new GameNotFoundException($"Game with ID {betDTO.GameId} not found.");
            return game;
        }

        /// <summary>
        /// Validates the bet amount and currency against the game's requirements.
        /// </summary>
        /// <param name="betDTO">The bet details.</param>
        /// <param name="game">The game details.</param>
        /// <exception cref="InvalidOperationException">Thrown if the bet amount or currency is invalid.</exception>
        private void ValidateBetAmountAndCurrency(PlaceBetDTO betDTO, Game game)
        {
            if (betDTO.Amount < game.MinimalBetAmount)
                throw new BetValidationException($"Bet amount must be at least {game.MinimalBetAmount}.");

            if (betDTO.CurrencyId != game.MinimalBetCurrencyId)
                throw new InvalidCurrencyException("Bet currency does not match game's minimal bet currency.");
        }

        /// <summary>
        /// Validates the wallet balance to ensure it is sufficient for the bet amount.
        /// </summary>
        /// <param name="wallet">The wallet details.</param>
        /// <param name="amount">The bet amount.</param>
        /// <exception cref="InvalidOperationException">Thrown if the wallet balance is insufficient.</exception>
        private void ValidateWalletBalance(Domain.Entities.Wallet wallet, decimal amount)
        {
            if (wallet.Balance < amount)
                throw new InsufficientBalanceException("Insufficient wallet balance.");
        }

        /// <summary>
        /// Creates a bet entity from the provided bet details.
        /// </summary>
        /// <param name="betDTO">The bet details.</param>
        /// <returns>The created bet entity.</returns>
        private Bet CreateBetEntity(PlaceBetDTO betDTO)
        {
            var betEntity = _mapper.Map<Bet>(betDTO);
            betEntity.StatusId = (int)BetStatusEnum.Created;
            return betEntity;
        }

        /// <summary>
        /// Determines whether the player wins the bet based on random logic.
        /// </summary>
        /// <returns>True if the player wins; otherwise, false.</returns>
        private bool DeterminePlayerWin()
        {
            return _randomService.GetRandomBoolean();
        }

        private async Task<int> GetConsecutiveLostBetCountAsync(Guid walletId, Guid gameId)
        {
            var bets = await _walletService.GetBetsByWalletIdAsync(walletId);
            return bets
                .Where(bet => bet.GameId == gameId)
                .TakeWhile(bet => bet.StatusId == (int)BetStatusEnum.Settled && !bet.IsWon)
                .Count();
        }

        /// <summary>
        /// Tracks the number of consecutive lost bets for a wallet and awards a bonus after every 5 losses.
        /// </summary>
        /// <param name="walletId">The ID of the wallet.</param>
        /// <param name="amount">The bet amount.</param>
        private async Task TrackLostBetAsync(Guid walletId, decimal amount, Guid gameId)
        {
            var game = await _gameService.GetGameByIdAsync(gameId);
            if (game == null) return;

            var lostBetCount = await GetConsecutiveLostBetCountAsync(walletId, gameId);

            if (game.ConsecutiveLossBonusThreshold.HasValue && lostBetCount == game.ConsecutiveLossBonusThreshold.Value)
            {
                var bonusAmount = Math.Round(amount * game.ConsecutiveLossBonusPercentage, 2);
                var wallet = await _walletService.GetWalletByIdAsync(walletId);
                if (wallet != null)
                {
                    await _walletService.CreditWalletAsync(wallet, bonusAmount, Guid.NewGuid());
                    _logger.LogInformation("Awarded bonus of {BonusAmount} to WalletId: {WalletId} at the {Threshold}th consecutive loss for GameId: {GameId}.", 
                        bonusAmount, walletId, game.ConsecutiveLossBonusThreshold, gameId);
                }
            }
        }
    }
}

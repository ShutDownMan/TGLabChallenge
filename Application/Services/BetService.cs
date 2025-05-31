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
        private readonly ILogger<BetService> _logger;
        private readonly IGameService _gameService;

        public BetService(
            IBetRepository betRepository,
            IMapper mapper,
            IWalletService walletService,
            IWalletTransactionService walletTransactionService,
            ILogger<BetService> logger,
            IGameService gameService)
        {
            _betRepository = betRepository;
            _mapper = mapper;
            _walletService = walletService;
            _walletTransactionService = walletTransactionService;
            _logger = logger;
            _gameService = gameService;
        }

        public async Task<BetDTO> PlaceBetAsync(PlaceBetDTO betDTO)
        {
            _logger.LogDebug("Starting PlaceBetAsync for WalletId: {WalletId}, Amount: {Amount}", betDTO.WalletId, betDTO.Amount);

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

            if (wallet.Balance < betDTO.Amount)
            {
                _logger.LogWarning("Insufficient wallet balance. WalletId: {WalletId}, Balance: {Balance}, BetAmount: {Amount}", wallet.Id, wallet.Balance, betDTO.Amount);
                throw new InvalidOperationException("Insufficient wallet balance.");
            }

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var betEntity = _mapper.Map<Bet>(betDTO);

                // Set status to Created
                betEntity.StatusId = (int)BetStatusEnum.Created;

                // Add bet to repository
                var newBetEntity = await _betRepository.AddAsync(betEntity);
                if (newBetEntity == null)
                {
                    _logger.LogError("Failed to add bet. WalletId: {WalletId}, Amount: {Amount}", betDTO.WalletId, betDTO.Amount);
                    throw new InvalidOperationException("Failed to place bet.");
                }

                _logger.LogDebug("Bet added. BetId: {BetId}, WalletId: {WalletId}", betEntity.Id, wallet.Id);

                // Debit wallet using service (handles balance and transaction)
                await _walletService.DebitWalletAsync(wallet, betDTO.Amount, newBetEntity.Id);

                _logger.LogDebug("Debited wallet. WalletId: {WalletId}, NewBalance: {Balance}", wallet.Id, wallet.Balance);

                // Fetch the refreshed wallet to ensure we have the latest balance
                var refreshedWallet = await _walletService.GetWalletByIdAsync(wallet.Id);
                if (refreshedWallet == null)
                {
                    _logger.LogWarning("Refreshed wallet not found. WalletId: {WalletId}", wallet.Id);
                    throw new InvalidOperationException("Refreshed wallet not found.");
                }

                _logger.LogDebug("Fetched refreshed wallet. WalletId: {WalletId}, Balance: {Balance}", refreshedWallet.Id, refreshedWallet.Balance);

                scope.Complete();

                _logger.LogInformation("PlaceBetAsync completed successfully for BetId: {BetId}", betEntity.Id);

                return new BetDTO
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
                // Get bet
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

    }
}

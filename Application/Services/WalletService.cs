using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Services
{
    /// <summary>
    /// Service for managing wallets, including debits, credits, and retrieving wallet information.
    /// </summary>
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IUserNotificationService _userNotificationService;

        public WalletService(IWalletRepository walletRepository, IWalletTransactionService walletTransactionService, IUserNotificationService userNotificationService)
        {
            _walletRepository = walletRepository;
            _walletTransactionService = walletTransactionService;
            _userNotificationService = userNotificationService;
        }

        /// <summary>
        /// Retrieves all wallets associated with a specific player asynchronously.
        /// </summary>
        /// <param name="playerId">The ID of the player.</param>
        /// <returns>A list of wallets.</returns>
        public async Task<List<Wallet>> GetWalletsByPlayerIdAsync(Guid playerId)
        {
            return await _walletRepository.GetWalletsByPlayerIdAsync(playerId);
        }

        /// <summary>
        /// Retrieves the player ID associated with a specific wallet asynchronously.
        /// </summary>
        /// <param name="walletId">The ID of the wallet.</param>
        /// <returns>The player ID, or null if not found.</returns>
        public async Task<Guid?> GetPlayerByWalletIdAsync(Guid walletId)
        {
            return await _walletRepository.GetPlayerByWalletIdAsync(walletId);
        }

        /// <summary>
        /// Retrieves a wallet by its ID asynchronously.
        /// </summary>
        /// <param name="walletId">The ID of the wallet.</param>
        /// <returns>The wallet, or null if not found.</returns>
        public async Task<Wallet?> GetWalletByIdAsync(Guid walletId)
        {
            return await _walletRepository.GetByIdAsync(walletId);
        }

        /// <summary>
        /// Checks if a wallet exists asynchronously.
        /// </summary>
        /// <param name="walletId">The ID of the wallet.</param>
        /// <returns>True if the wallet exists, otherwise false.</returns>
        public async Task<bool> WalletExistsAsync(Guid walletId)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);
            return wallet != null;
        }

        /// <summary>
        /// Updates a wallet asynchronously.
        /// </summary>
        /// <param name="wallet">The wallet to update.</param>
        public async Task UpdateWalletAsync(Wallet wallet)
        {
            await _walletRepository.UpdateAsync(wallet);
        }

        /// <summary>
        /// Debits a wallet asynchronously.
        /// </summary>
        /// <param name="wallet">The wallet to debit.</param>
        /// <param name="amount">The amount to debit.</param>
        /// <param name="betId">Optional bet ID associated with the transaction.</param>
        /// <returns>The created wallet transaction.</returns>
        public async Task<WalletTransaction> DebitWalletAsync(Wallet wallet, decimal amount, Guid? betId = null)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                wallet.DebitWallet(amount);
                var transaction = await _walletTransactionService.DebitWalletAsync(wallet, amount, betId);

                // Notify user about wallet debit
                var playerId = await GetPlayerByWalletIdAsync(wallet.Id);
                if (playerId != null)
                {
                    var transactionDto = new WalletTransactionDTO
                    {
                        Id = transaction.Id,
                        WalletId = transaction.WalletId,
                        Amount = transaction.Amount,
                        TypeId = transaction.TransactionTypeId,
                        CreatedAt = transaction.CreatedAt,
                        BetId = transaction.BetId,
                    };
                    await _userNotificationService.NotifyWalletTransactionUpdateAsync(playerId.Value, transactionDto);
                }

                scope.Complete();
                return transaction;
            }
            catch
            {
                // Transaction will be rolled back if not completed
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }

        /// <summary>
        /// Credits a wallet asynchronously.
        /// </summary>
        /// <param name="wallet">The wallet to credit.</param>
        /// <param name="amount">The amount to credit.</param>
        /// <param name="betId">Optional bet ID associated with the transaction.</param>
        /// <returns>The created wallet transaction.</returns>
        public async Task<WalletTransaction> CreditWalletAsync(Wallet wallet, decimal amount, Guid? betId = null)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                wallet.CreditWallet(amount);
                var transaction = await _walletTransactionService.CreditWalletAsync(wallet, amount, betId);

                // Notify user about wallet credit
                var playerId = await GetPlayerByWalletIdAsync(wallet.Id);
                if (playerId != null)
                {
                    var transactionDto = new WalletTransactionDTO
                    {
                        Id = transaction.Id,
                        WalletId = transaction.WalletId,
                        Amount = transaction.Amount,
                        TypeId = transaction.TransactionTypeId,
                        CreatedAt = transaction.CreatedAt,
                        BetId = transaction.BetId,
                    };
                    await _userNotificationService.NotifyWalletTransactionUpdateAsync(playerId.Value, transactionDto);
                }

                scope.Complete();
                return transaction;
            }
            catch
            {
                // Transaction will be rolled back if not completed
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }

        /// <summary>
        /// Calculates and creates a checkpoint transaction for a wallet asynchronously.
        /// </summary>
        /// <param name="wallet">The wallet to create a checkpoint for.</param>
        /// <returns>The created checkpoint transaction.</returns>
        public async Task<WalletTransaction> CalculateWalletCheckpointAsync(Wallet wallet)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var transaction = await _walletTransactionService.CalculateWalletCheckpointAsync(wallet.Id);
                scope.Complete();
                return transaction;
            }
            catch
            {
                // Transaction will be rolled back if not completed
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }

        /// <summary>
        /// Retrieves the currency associated with a specific wallet asynchronously.
        /// </summary>
        /// <param name="walletId">The ID of the wallet.</param>
        /// <returns>The currency, or null if not found.</returns>
        public async Task<Currency?> GetCurrencyByWalletIdAsync(Guid walletId)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);
            if (wallet == null)
                return null;

            return wallet.Currency;
        }
    }
}

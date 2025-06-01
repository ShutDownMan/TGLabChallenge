using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TransactionTypeEnum = Domain.Enums.TransactionType;

namespace Application.Services
{
    /// <summary>
    /// Service for managing wallet transactions, including debits, credits, and checkpoints.
    /// </summary>
    public class WalletTransactionService : IWalletTransactionService
    {
        private readonly IWalletTransactionRepository _walletTransactionRepository;

        public WalletTransactionService(IWalletTransactionRepository walletTransactionRepository)
        {
            _walletTransactionRepository = walletTransactionRepository;
        }

        /// <summary>
        /// Adds a new wallet transaction asynchronously.
        /// </summary>
        /// <param name="transaction">The wallet transaction to add.</param>
        public async Task AddAsync(WalletTransaction transaction)
        {
            await _walletTransactionRepository.AddAsync(transaction);
        }

        /// <summary>
        /// Retrieves all transactions for a specific wallet asynchronously.
        /// </summary>
        /// <param name="walletId">The ID of the wallet.</param>
        /// <returns>A collection of wallet transactions.</returns>
        public async Task<IEnumerable<WalletTransaction>> GetByWalletIdAsync(Guid walletId)
        {
            return await _walletTransactionRepository.GetByWalletIdAsync(walletId);
        }

        /// <summary>
        /// Retrieves transaction information for a specific wallet asynchronously.
        /// </summary>
        /// <param name="walletId">The ID of the wallet.</param>
        /// <param name="pageNumber">The page number for pagination.</param>
        /// <param name="pageSize">The number of items per page for pagination.</param>
        /// <returns>A collection of wallet transaction DTOs.</returns>
        public async Task<IEnumerable<WalletTransactionDTO>> GetTransactionInfosByWalletIdAsync(Guid walletId, int pageNumber, int pageSize)
        {
            var transactions = await _walletTransactionRepository.GetByWalletIdAsync(walletId);
            var paginatedTransactions = transactions
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return paginatedTransactions.Select(tx => new WalletTransactionDTO
            {
                Id = tx.Id,
                WalletId = tx.WalletId,
                CreatedAt = tx.CreatedAt,
                TypeId = tx.TransactionTypeId,
                Amount = tx.Amount
            });
        }

        /// <summary>
        /// Creates a debit transaction for a wallet asynchronously.
        /// </summary>
        /// <param name="wallet">The wallet to debit.</param>
        /// <param name="amount">The amount to debit.</param>
        /// <param name="betId">Optional bet ID associated with the transaction.</param>
        /// <returns>The created wallet transaction.</returns>
        public async Task<WalletTransaction> DebitWalletAsync(Wallet wallet, decimal amount, Guid? betId = null)
        {
            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                BetId = betId,
                TransactionTypeId = (int)TransactionTypeEnum.Debit,
                Amount = Math.Abs(amount),
                CreatedAt = DateTime.UtcNow
            };
            await _walletTransactionRepository.AddAsync(transaction);
            return transaction;
        }

        /// <summary>
        /// Creates a credit transaction for a wallet asynchronously.
        /// </summary>
        /// <param name="wallet">The wallet to credit.</param>
        /// <param name="amount">The amount to credit.</param>
        /// <param name="betId">Optional bet ID associated with the transaction.</param>
        /// <returns>The created wallet transaction.</returns>
        public async Task<WalletTransaction> CreditWalletAsync(Wallet wallet, decimal amount, Guid? betId = null)
        {
            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                BetId = betId,
                TransactionTypeId = (int)TransactionTypeEnum.Credit,
                Amount = Math.Abs(amount),
                CreatedAt = DateTime.UtcNow
            };
            await _walletTransactionRepository.AddAsync(transaction);
            return transaction;
        }

        /// <summary>
        /// Creates a checkpoint transaction for a wallet asynchronously.
        /// </summary>
        /// <param name="wallet">The wallet to create a checkpoint for.</param>
        /// <param name="checkpointAmount">The checkpoint amount.</param>
        /// <param name="parentCheckpointId">Optional parent checkpoint ID.</param>
        /// <returns>The created wallet transaction.</returns>
        public async Task<WalletTransaction> CheckpointWalletAsync(Wallet wallet, decimal checkpointAmount, Guid? parentCheckpointId = null)
        {
            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                TransactionTypeId = (int)TransactionTypeEnum.Checkpoint,
                Amount = checkpointAmount,
                CreatedAt = DateTime.UtcNow,
                ParentWalletTransactionCheckpointId = parentCheckpointId
            };
            await _walletTransactionRepository.AddAsync(transaction);
            return transaction;
        }

        /// <summary>
        /// Calculates and creates a checkpoint transaction for a wallet asynchronously.
        /// </summary>
        /// <param name="walletId">The ID of the wallet.</param>
        /// <returns>The created checkpoint transaction.</returns>
        public async Task<WalletTransaction> CalculateWalletCheckpointAsync(Guid walletId)
        {
            using var scope = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var transactions = (await _walletTransactionRepository.GetByWalletIdAsync(walletId)).ToList();

                var lastCheckpoint = transactions
                    .Where(tx => tx.TransactionTypeId == (int)TransactionTypeEnum.Checkpoint)
                    .OrderByDescending(tx => tx.CreatedAt)
                    .FirstOrDefault();

                DateTime? lastCheckpointTime = lastCheckpoint?.CreatedAt;

                var checkpoint = transactions
                    .Where(tx => lastCheckpointTime == null || tx.CreatedAt > lastCheckpointTime)
                    .Sum(tx =>
                        tx.TransactionTypeId == (int)TransactionTypeEnum.Credit ? tx.Amount :
                        tx.TransactionTypeId == (int)TransactionTypeEnum.Debit ? -tx.Amount : 0m
                    );

                var checkpointTransaction = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = walletId,
                    TransactionTypeId = (int)TransactionTypeEnum.Checkpoint,
                    Amount = checkpoint,
                    CreatedAt = DateTime.UtcNow,
                    ParentWalletTransactionCheckpointId = lastCheckpoint?.Id
                };

                await _walletTransactionRepository.AddAsync(checkpointTransaction);

                scope.Complete();
                return checkpointTransaction;
            }
            catch
            {
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }
    }
}

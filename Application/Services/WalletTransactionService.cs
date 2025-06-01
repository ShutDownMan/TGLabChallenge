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
    public class WalletTransactionService : IWalletTransactionService
    {
        private readonly IWalletTransactionRepository _walletTransactionRepository;

        public WalletTransactionService(IWalletTransactionRepository walletTransactionRepository)
        {
            _walletTransactionRepository = walletTransactionRepository;
        }

        public async Task AddAsync(WalletTransaction transaction)
        {
            await _walletTransactionRepository.AddAsync(transaction);
        }

        public async Task<IEnumerable<WalletTransaction>> GetByWalletIdAsync(Guid walletId)
        {
            return await _walletTransactionRepository.GetByWalletIdAsync(walletId);
        }

        public async Task<IEnumerable<WalletTransactionDTO>> GetTransactionInfosByWalletIdAsync(Guid walletId)
        {
            var transactions = await _walletTransactionRepository.GetByWalletIdAsync(walletId);
            return transactions.Select(tx => new WalletTransactionDTO
            {
                Id = tx.Id,
                WalletId = tx.WalletId,
                CreatedAt = tx.CreatedAt,
                TypeId = tx.TransactionTypeId,
                Amount = tx.Amount
            });
        }

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

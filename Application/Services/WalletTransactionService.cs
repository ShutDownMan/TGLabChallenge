using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task<WalletTransaction> DebitWalletAsync(Wallet wallet, decimal amount, Guid? betId = null)
        {
            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                BetId = betId,
                TransactionTypeId = (int)TransactionType.Debit,
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
                TransactionTypeId = (int)TransactionType.Credit,
                Amount = Math.Abs(amount),
                CreatedAt = DateTime.UtcNow
            };
            await _walletTransactionRepository.AddAsync(transaction);
            return transaction;
        }

        public async Task<WalletTransaction> CheckpointWalletAsync(Wallet wallet, decimal checkpointAmount)
        {
            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                WalletId = wallet.Id,
                TransactionTypeId = (int)TransactionType.Checkpoint,
                Amount = checkpointAmount,
                CreatedAt = DateTime.UtcNow
            };
            await _walletTransactionRepository.AddAsync(transaction);
            return transaction;
        }
    }
}

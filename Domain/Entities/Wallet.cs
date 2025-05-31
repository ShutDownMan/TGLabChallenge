using System;
using System.Collections.Generic;
using Domain.Enums;

using TransactionTypeEnum = Domain.Enums.TransactionType;

namespace Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public int CurrencyId { get; set; }
        public decimal Balance { get; set; } = 0.00m;
        public DateTime CreatedAt { get; set; }

        public Player? Player { get; set; }
        public Currency? Currency { get; set; }
        public List<WalletTransaction> WalletTransactions { get; set; } = new();

        public void DebitWallet(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));
            if (Balance < amount)
                throw new InvalidOperationException("Insufficient wallet balance.");
            Balance -= amount;
        }

        public void CreditWallet(decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive.", nameof(amount));
            Balance += amount;
        }

        public decimal CalculateWalletCheckpoint()
        {
            // Find the last checkpoint transaction
            var lastCheckpoint = WalletTransactions
                .Where(tx => tx.TransactionTypeId == (int)TransactionTypeEnum.Checkpoint)
                .OrderByDescending(tx => tx.CreatedAt)
                .FirstOrDefault();

            DateTime? lastCheckpointTime = lastCheckpoint?.CreatedAt;

            decimal checkpoint = 0m;
            foreach (var tx in WalletTransactions)
            {
                if (lastCheckpointTime != null && tx.CreatedAt <= lastCheckpointTime)
                    continue;

                if (tx.TransactionTypeId == (int)TransactionTypeEnum.Debit)
                {
                    checkpoint -= tx.Amount;
                }
                else if (tx.TransactionTypeId == (int)TransactionTypeEnum.Credit)
                {
                    checkpoint += tx.Amount;
                }
            }
            return checkpoint;
        }
    }
}

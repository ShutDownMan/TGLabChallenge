using System;
using System.Collections.Generic;
using Domain.Enums;

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
    }
}

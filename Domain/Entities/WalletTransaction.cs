using System;

namespace Domain.Entities
{
    public class WalletTransaction
    {
        public Guid Id { get; set; }
        public Guid PlayerId { get; set; }
        public int TransactionTypeId { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public DateTime CreatedAt { get; set; }

        public Player? Player { get; set; }
        public TransactionType? TransactionType { get; set; }
        public Currency? Currency { get; set; }
    }
}

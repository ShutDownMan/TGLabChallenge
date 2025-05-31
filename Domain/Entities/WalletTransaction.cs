using System;

namespace Domain.Entities
{
    public class WalletTransaction
    {
        public Guid Id { get; set; }
        public Guid? BetId { get; set; }
        public Guid WalletId { get; set; }
        public int TransactionTypeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }

        public Wallet? Wallet { get; set; }
        public Bet? Bet { get; set; }
        public TransactionType? TransactionType { get; set; }
    }
}

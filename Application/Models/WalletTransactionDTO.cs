using System;

namespace Application.Models
{
    public class WalletTransactionDTO
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TypeId { get; set; }
        public decimal Amount { get; set; }
        public Guid? BetId { get; set; }
    }
}

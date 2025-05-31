using System;

namespace Application.Models
{
    public class BetDTO
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public int StatusId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? Payout { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid GameId { get; set; }
    }
}

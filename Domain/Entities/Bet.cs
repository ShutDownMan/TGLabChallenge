using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Bet
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public decimal Amount { get; set; }
        public int StatusId { get; set; }
        public decimal? Payout { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid GameId { get; set; }
        public string? Note { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        public Wallet? Wallet { get; set; }
        public BetStatus? Status { get; set; }
        public Game? Game { get; set; }
    }
}

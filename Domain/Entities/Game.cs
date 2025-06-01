using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Game
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal MinimalBetAmount { get; set; } = 0.00m;
        public int MinimalBetCurrencyId { get; set; }
        public decimal CancelTaxPercentage { get; set; } = 0.00m;
        public DateTime CreatedAt { get; set; }
        public int? ConsecutiveLossBonusThreshold { get; set; }
        public decimal ConsecutiveLossBonusPercentage { get; set; } = 0.10m;
        public decimal Odds { get; set; } = 2.00m;

        public List<Bet> Bets { get; set; } = new();
    }
}

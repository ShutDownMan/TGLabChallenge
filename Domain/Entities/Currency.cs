using System.Collections.Generic;

namespace Domain.Entities
{
    public class Currency
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public List<Player> Players { get; set; } = new();
        public List<Bet> Bets { get; set; } = new();
        public List<WalletTransaction> WalletTransactions { get; set; } = new();
    }
}

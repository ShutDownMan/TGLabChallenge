using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Player
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Name
        {
            get => Username;
            set => Username = value;
        }
        public string Email { get; set; } = string.Empty;
        public int CurrencyId { get; set; }
        public decimal Balance { get; set; } = 0.00m;
        public DateTime CreatedAt { get; set; }

        public Currency? Currency { get; set; }
        public List<WalletTransaction> WalletTransactions { get; set; } = new();
        public List<Bet> Bets { get; set; } = new List<Bet>();
    }
}

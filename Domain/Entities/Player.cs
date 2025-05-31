using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Player
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public List<Bet> Bets { get; set; } = new List<Bet>();
    }
}

using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Game
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<Bet> Bets { get; set; } = new();
    }
}

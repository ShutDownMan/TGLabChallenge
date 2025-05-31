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
        public Guid PlayerId { get; set; }
        public decimal Amount { get; set; }
        public int StatusId { get; set; }
        public decimal? Prize { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid GameId { get; set; }

        public Player? Player { get; set; }
        public BetStatus? Status { get; set; }
        public Game? Game { get; set; }
    }
}

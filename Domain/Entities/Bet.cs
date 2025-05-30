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
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public BetStatus Status { get; set; } = BetStatus.Created;

        // FK
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}

using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BetRepository : IBetRepository
    {
        private readonly AppDbContext _context;

        public BetRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Bet> GetByIdAsync(Guid id)
        {
            return await _context.Bets
                .Include(b => b.Player)
                .FirstAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Bet>> GetByUserAsync(Guid userId)
        {
            return await _context.Bets
                .Where(b => b.PlayerId == userId)
                .ToListAsync();
        }

        public async Task AddAsync(Bet bet)
        {
            _context.Bets.Add(bet);
            await _context.SaveChangesAsync();
        }

        public async Task CancelAsync(Bet bet)
        {
            // Assuming "Cancel" means updating the status
            _context.Bets.Update(bet);
            await _context.SaveChangesAsync();
        }
    }
}

using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using BetStatusEnum = Domain.Enums.BetStatus;

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
                .Include(b => b.Wallet)
                .FirstAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Bet>> GetByUserAsync(Guid userId)
        {
            return await _context.Bets
                .Include(b => b.Wallet)
                .Where(b => b.Wallet != null && b.Wallet.PlayerId == userId)
                .ToListAsync();
        }

        public async Task<Bet> AddAsync(Bet bet)
        {
            bet.Id = Guid.NewGuid();
            bet.CreatedAt = DateTime.UtcNow;

            _context.Bets.Add(bet);
            await _context.SaveChangesAsync();

            return bet;
        }

        public async Task<Bet> UpdateAsync(Bet bet)
        {
            _context.Bets.Update(bet);
            await _context.SaveChangesAsync();
            return bet;
        }

        public async Task CancelAsync(Guid betId)
        {
            var bet = await GetByIdAsync(betId);
            if (bet == null)
            {
                throw new KeyNotFoundException($"Bet with ID {betId} not found.");
            }

            bet.StatusId = (int)BetStatusEnum.Cancelled;

            _context.Bets.Update(bet);

            await _context.SaveChangesAsync();
        }
    }
}

using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly AppDbContext _context;

        public PlayerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Player?> GetByUsernameAsync(string username)
        {
            return await _context.Players
                .Include(u => u.Bets)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<Player?> GetByEmailAsync(string email)
        {
            return await _context.Players
                .Include(u => u.Bets)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Player> GetByIdAsync(Guid id)
        {
            return await _context.Players
                .Include(u => u.Bets)
                .FirstAsync(u => u.Id == id);
        }

        public async Task AddAsync(Player user)
        {
            _context.Players.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Players.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Players.AnyAsync(u => u.Email == email);
        }
    }
}

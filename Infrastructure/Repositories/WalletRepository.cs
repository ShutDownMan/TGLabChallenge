using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly AppDbContext _context;

        public WalletRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Wallet wallet)
        {
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Wallet>> GetWalletsByPlayerIdAsync(Guid playerId)
        {
            return await _context.Wallets
                .Include(w => w.Currency)
                .Where(w => w.PlayerId == playerId)
                .ToListAsync();
        }

        public async Task UpdateAsync(Wallet wallet)
        {
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();
        }

        public async Task<Guid?> GetPlayerByWalletIdAsync(Guid walletId)
        {
            var wallet = await _context.Wallets
                .Where(w => w.Id == walletId)
                .Select(w => w.PlayerId)
                .FirstOrDefaultAsync();

            // If not found, FirstOrDefaultAsync returns default(Guid) which is Guid.Empty
            return wallet == Guid.Empty ? (Guid?)null : wallet;
        }
    }
}

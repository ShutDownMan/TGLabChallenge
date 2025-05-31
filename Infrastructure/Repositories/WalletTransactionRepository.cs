using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class WalletTransactionRepository : IWalletTransactionRepository
    {
        private readonly AppDbContext _context;

        public WalletTransactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WalletTransaction> GetByIdAsync(Guid id)
        {
            return await _context.WalletTransactions
                .Include(wt => wt.Player)
                .Include(wt => wt.TransactionType)
                .Include(wt => wt.Currency)
                .FirstAsync(wt => wt.Id == id);
        }

        public async Task<IEnumerable<WalletTransaction>> GetByPlayerAsync(Guid playerId)
        {
            return await _context.WalletTransactions
                .Where(wt => wt.PlayerId == playerId)
                .ToListAsync();
        }

        public async Task AddAsync(WalletTransaction transaction)
        {
            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }
    }
}

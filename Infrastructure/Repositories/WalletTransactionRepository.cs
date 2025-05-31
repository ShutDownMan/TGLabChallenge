using Application.Interfaces.Repositories;
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
                .Include(wt => wt.TransactionType)
                .FirstAsync(wt => wt.Id == id);
        }

        public async Task AddAsync(WalletTransaction transaction)
        {
            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WalletTransaction>> GetByWalletIdAsync(Guid walletId)
        {
            return await _context.WalletTransactions
                .Include(wt => wt.TransactionType)
                .Where(wt => wt.WalletId == walletId)
                .ToListAsync();
        }
    }
}

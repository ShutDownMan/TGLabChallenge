using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<BetStatus> BetStatuses { get; set; }
        public DbSet<TransactionType> TransactionTypes { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Wallet> Wallets { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Currency>().HasData(
                new Currency { Id = 1, Code = "USD", Name = "US Dollar" },
                new Currency { Id = 2, Code = "EUR", Name = "Euro" },
                new Currency { Id = 3, Code = "BRL", Name = "Brazilian Real" }
            );

            modelBuilder.Entity<BetStatus>().HasData(
                new BetStatus { Id = 1, Name = "Created" },
                new BetStatus { Id = 2, Name = "Cancelled" },
                new BetStatus { Id = 3, Name = "Settled" }
            );

            modelBuilder.Entity<TransactionType>().HasData(
                new TransactionType { Id = 1, Name = "Deposit" },
                new TransactionType { Id = 2, Name = "Withdrawal" }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.AmbientTransactionWarning));
        }
    }

}

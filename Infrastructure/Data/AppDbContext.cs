using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

using BetStatusEnum = Domain.Enums.BetStatus;
using BetStatus = Domain.Entities.BetStatus;
using TransactionTypeEnum = Domain.Enums.TransactionType;
using TransactionType = Domain.Entities.TransactionType;

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
                new BetStatus { Id = (int)BetStatusEnum.Created, Name = "Created" },
                new BetStatus { Id = (int)BetStatusEnum.Cancelled, Name = "Cancelled" },
                new BetStatus { Id = (int)BetStatusEnum.Settled, Name = "Settled" }
            );

            modelBuilder.Entity<TransactionType>().HasData(
                new TransactionType { Id = (int)TransactionTypeEnum.Debit, Name = "Debit" },
                new TransactionType { Id = (int)TransactionTypeEnum.Credit, Name = "Credit" },
                new TransactionType { Id = (int)TransactionTypeEnum.Checkpoint, Name = "Checkpoint" }
            );

            // Placeholder Game entity data
            modelBuilder.Entity<Game>().HasData(
                new Game { Id = Guid.Parse("7558398b-a987-4b88-9010-c026306d3535"), Name = "Placeholder Game", Description = "This is a placeholder game for testing purposes." }
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

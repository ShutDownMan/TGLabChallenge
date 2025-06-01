using Xunit;
using Moq;
using Application.Services;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Application.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.Services
{
    public class PlayerServiceTests
    {
        [Fact]
        public async Task GetProfileAsync_WithExistingPlayer_ReturnsProfile()
        {
            #region Arrange
            var playerId = Guid.NewGuid();
            var playerRepo = new Mock<IPlayerRepository>();
            var walletService = new Mock<IWalletService>();
            var betService = new Mock<IBetService>();
            var walletTransactionService = new Mock<IWalletTransactionService>();
            var logger = new Mock<ILogger<PlayerService>>();

            var player = new Player { Id = playerId, Username = "test", Email = "test@email.com", CreatedAt = DateTime.UtcNow };
            playerRepo.Setup(r => r.GetByIdAsync(playerId)).ReturnsAsync(player);

            var wallets = new List<Wallet>
            {
                new Wallet { Id = Guid.NewGuid(), CurrencyId = 100, Balance = 100, CreatedAt = DateTime.UtcNow }
            };
            walletService.Setup(w => w.GetWalletsByPlayerIdAsync(playerId)).ReturnsAsync(wallets);

            var service = new PlayerService(
                playerRepo.Object,
                walletService.Object,
                betService.Object,
                walletTransactionService.Object,
                logger.Object
            );
            #endregion

            #region Act
            var profile = await service.GetProfileAsync(playerId);
            #endregion

            #region Assert
            Assert.NotNull(profile);
            Assert.Equal("test", profile!.Username);
            Assert.Equal(player.Email, profile.Email);
            Assert.Single(profile.Wallets);
            #endregion
        }

        [Fact]
        public async Task GetProfileAsync_WithNonExistingPlayer_ReturnsNull()
        {
            #region Arrange
            var playerRepo = new Mock<IPlayerRepository>();
            var walletService = new Mock<IWalletService>();
            var betService = new Mock<IBetService>();
            var walletTransactionService = new Mock<IWalletTransactionService>();
            var logger = new Mock<ILogger<PlayerService>>();

            playerRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Player?)null);

            var service = new PlayerService(
                playerRepo.Object,
                walletService.Object,
                betService.Object,
                walletTransactionService.Object,
                logger.Object
            );
            #endregion

            #region Act
            var profile = await service.GetProfileAsync(Guid.NewGuid());
            #endregion

            #region Assert
            Assert.Null(profile);
            #endregion
        }

        [Fact]
        public async Task GetBetsAsync_WithExistingPlayer_ReturnsBets()
        {
            #region Arrange
            var playerId = Guid.NewGuid();
            var playerRepo = new Mock<IPlayerRepository>();
            var walletService = new Mock<IWalletService>();
            var betService = new Mock<IBetService>();
            var walletTransactionService = new Mock<IWalletTransactionService>();
            var logger = new Mock<ILogger<PlayerService>>();

            var bets = new List<BetDTO>
            {
                new BetDTO { Id = Guid.NewGuid(), Amount = 100, Status = "Created", CreatedAt = DateTime.UtcNow }
            };
            betService.Setup(b => b.GetBetsByUserAsync(playerId)).ReturnsAsync(bets);

            var service = new PlayerService(
                playerRepo.Object,
                walletService.Object,
                betService.Object,
                walletTransactionService.Object,
                logger.Object
            );
            #endregion

            #region Act
            var result = await service.GetBetsAsync(playerId);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(100, result.First().Amount);
            #endregion
        }

        [Fact]
        public async Task GetWalletTransactionsAsync_WithExistingPlayer_ReturnsTransactions()
        {
            #region Arrange
            var playerId = Guid.NewGuid();
            var playerRepo = new Mock<IPlayerRepository>();
            var walletService = new Mock<IWalletService>();
            var betService = new Mock<IBetService>();
            var walletTransactionService = new Mock<IWalletTransactionService>();
            var logger = new Mock<ILogger<PlayerService>>();

            var wallets = new List<Wallet>
            {
                new Wallet { Id = Guid.NewGuid(), CurrencyId = 100, Balance = 100, CreatedAt = DateTime.UtcNow }
            };
            walletService.Setup(w => w.GetWalletsByPlayerIdAsync(playerId)).ReturnsAsync(wallets);

            var transactions = new List<WalletTransactionDTO>
            {
                new WalletTransactionDTO { Id = Guid.NewGuid(), Amount = 50, CreatedAt = DateTime.UtcNow }
            };
            walletTransactionService.Setup(w => w.GetTransactionInfosByWalletIdAsync(wallets.First().Id)).ReturnsAsync(transactions);

            var service = new PlayerService(
                playerRepo.Object,
                walletService.Object,
                betService.Object,
                walletTransactionService.Object,
                logger.Object
            );
            #endregion

            #region Act
            var result = await service.GetWalletTransactionsAsync(playerId);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(50, result.First().Amount);
            #endregion
        }
    }
}

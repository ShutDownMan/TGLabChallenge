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

            var profile = await service.GetProfileAsync(playerId);

            Assert.NotNull(profile);
            Assert.Equal("test", profile!.Username);
            Assert.Equal(player.Email, profile.Email);
            Assert.Single(profile.Wallets);
        }

        [Fact]
        public async Task GetProfileAsync_WithNonExistingPlayer_ReturnsNull()
        {
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

            var profile = await service.GetProfileAsync(Guid.NewGuid());

            Assert.Null(profile);
        }
    }
}

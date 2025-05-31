using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Application.Services;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AutoMapper;

using BetStatusEnum = Domain.Enums.BetStatus;

namespace Tests.Services
{
    public class BetServiceTests
    {
        private readonly Mock<IBetRepository> _betRepo = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<IWalletService> _walletService = new();
        private readonly Mock<IWalletTransactionService> _walletTxService = new();
        private readonly Mock<IGameService> _gameService = new();
        private readonly Mock<ILogger<BetService>> _logger = new();

        private BetService CreateService() =>
            new BetService(_betRepo.Object, _mapper.Object, _walletService.Object, _walletTxService.Object, _gameService.Object, _logger.Object);

        [Fact]
        public async Task PlaceBetAsync_WithValidData_PlacesBetAndDebitsWallet()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var betId = Guid.NewGuid();

            var wallet = new Wallet { Id = walletId, Balance = 100, CurrencyId = 1 };
            var game = new Game { Id = gameId, MinimalBetAmount = 10, MinimalBetCurrencyId = 1 };
            var betDTO = new PlaceBetDTO { WalletId = walletId, Amount = 20, CurrencyId = 1, GameId = gameId };
            var betEntity = new Bet { Id = betId, WalletId = walletId, Amount = 20, StatusId = (int)BetStatusEnum.Created, GameId = gameId };
            var newBetEntity = new Bet { Id = betId, WalletId = walletId, Amount = 20, StatusId = (int)BetStatusEnum.Created, GameId = gameId };

            _walletService.Setup(s => s.GetPlayerByWalletIdAsync(walletId)).ReturnsAsync(playerId);
            _walletService.Setup(s => s.GetWalletsByPlayerIdAsync(playerId)).ReturnsAsync(new List<Wallet> { wallet });
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);
            _mapper.Setup(m => m.Map<Bet>(betDTO)).Returns(betEntity);
            _betRepo.Setup(r => r.AddAsync(It.IsAny<Bet>())).ReturnsAsync(newBetEntity);
            _walletService.Setup(s => s.DebitWalletAsync(wallet, betDTO.Amount, betId)).ReturnsAsync(new WalletTransaction());
            _walletService.Setup(s => s.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);

            var service = CreateService();
            #endregion

            #region Act
            var result = await service.PlaceBetAsync(betDTO);
            #endregion

            #region Assert
            Assert.Equal(betId, result.Id);
            Assert.Equal(walletId, result.WalletId);
            Assert.Equal(20, result.Amount);
            Assert.Equal((int)BetStatusEnum.Created, result.StatusId);
            #endregion
        }

        [Fact]
        public async Task PlaceBetAsync_PlayerNotFound_Throws()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var betDTO = new PlaceBetDTO { WalletId = walletId, Amount = 10, CurrencyId = 1, GameId = Guid.NewGuid() };
            _walletService.Setup(s => s.GetPlayerByWalletIdAsync(walletId)).ReturnsAsync((Guid?)null);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.PlaceBetAsync(betDTO));
            #endregion
        }

        [Fact]
        public async Task PlaceBetAsync_WalletNotFound_Throws()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var betDTO = new PlaceBetDTO { WalletId = walletId, Amount = 10, CurrencyId = 1, GameId = Guid.NewGuid() };
            _walletService.Setup(s => s.GetPlayerByWalletIdAsync(walletId)).ReturnsAsync(playerId);
            _walletService.Setup(s => s.GetWalletsByPlayerIdAsync(playerId)).ReturnsAsync(new List<Wallet>());

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.PlaceBetAsync(betDTO));
            #endregion
        }

        [Fact]
        public async Task PlaceBetAsync_GameNotFound_Throws()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var betDTO = new PlaceBetDTO { WalletId = walletId, Amount = 10, CurrencyId = 1, GameId = Guid.NewGuid() };
            var wallet = new Wallet { Id = walletId, Balance = 100, CurrencyId = 1 };
            _walletService.Setup(s => s.GetPlayerByWalletIdAsync(walletId)).ReturnsAsync(playerId);
            _walletService.Setup(s => s.GetWalletsByPlayerIdAsync(playerId)).ReturnsAsync(new List<Wallet> { wallet });
            _gameService.Setup(s => s.GetGameByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Game?)null);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.PlaceBetAsync(betDTO));
            #endregion
        }

        [Fact]
        public async Task PlaceBetAsync_AmountBelowMinimum_Throws()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var betDTO = new PlaceBetDTO { WalletId = walletId, Amount = 5, CurrencyId = 1, GameId = gameId };
            var wallet = new Wallet { Id = walletId, Balance = 100, CurrencyId = 1 };
            var game = new Game { Id = gameId, MinimalBetAmount = 10, MinimalBetCurrencyId = 1 };
            _walletService.Setup(s => s.GetPlayerByWalletIdAsync(walletId)).ReturnsAsync(playerId);
            _walletService.Setup(s => s.GetWalletsByPlayerIdAsync(playerId)).ReturnsAsync(new List<Wallet> { wallet });
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.PlaceBetAsync(betDTO));
            #endregion
        }

        [Fact]
        public async Task PlaceBetAsync_CurrencyMismatch_Throws()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var betDTO = new PlaceBetDTO { WalletId = walletId, Amount = 20, CurrencyId = 2, GameId = gameId };
            var wallet = new Wallet { Id = walletId, Balance = 100, CurrencyId = 2 };
            var game = new Game { Id = gameId, MinimalBetAmount = 10, MinimalBetCurrencyId = 1 };
            _walletService.Setup(s => s.GetPlayerByWalletIdAsync(walletId)).ReturnsAsync(playerId);
            _walletService.Setup(s => s.GetWalletsByPlayerIdAsync(playerId)).ReturnsAsync(new List<Wallet> { wallet });
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.PlaceBetAsync(betDTO));
            #endregion
        }

        [Fact]
        public async Task PlaceBetAsync_InsufficientBalance_Throws()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var betDTO = new PlaceBetDTO { WalletId = walletId, Amount = 200, CurrencyId = 1, GameId = gameId };
            var wallet = new Wallet { Id = walletId, Balance = 100, CurrencyId = 1 };
            var game = new Game { Id = gameId, MinimalBetAmount = 10, MinimalBetCurrencyId = 1 };
            _walletService.Setup(s => s.GetPlayerByWalletIdAsync(walletId)).ReturnsAsync(playerId);
            _walletService.Setup(s => s.GetWalletsByPlayerIdAsync(playerId)).ReturnsAsync(new List<Wallet> { wallet });
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.PlaceBetAsync(betDTO));
            #endregion
        }
    }
}

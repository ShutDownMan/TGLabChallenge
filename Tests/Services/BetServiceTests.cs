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
        private readonly Mock<IUserNotificationService> _userNotificationService = new();

        private BetService CreateService() =>
            new BetService(
                _betRepo.Object,
                _mapper.Object,
                _walletService.Object,
                _walletTxService.Object,
                _gameService.Object,
                _logger.Object,
                _userNotificationService.Object);

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

        [Fact]
        public async Task SettleBetAsync_BetNotFound_Throws()
        {
            #region Arrange
            var service = CreateService();
            _betRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Bet?)null);
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.SettleBetAsync(Guid.NewGuid()));
            #endregion
        }

        [Fact]
        public async Task SettleBetAsync_BetNotCreated_Throws()
        {
            #region Arrange
            var betId = Guid.NewGuid();
            var bet = new Bet { Id = betId, StatusId = (int)BetStatusEnum.Cancelled };
            _betRepo.Setup(r => r.GetByIdAsync(betId)).ReturnsAsync(bet);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SettleBetAsync(betId));
            #endregion
        }

        [Fact]
        public async Task SettleBetAsync_GameNotFound_Throws()
        {
            #region Arrange
            var betId = Guid.NewGuid();
            var bet = new Bet { Id = betId, StatusId = (int)BetStatusEnum.Created, GameId = Guid.NewGuid() };
            _betRepo.Setup(r => r.GetByIdAsync(betId)).ReturnsAsync(bet);
            _gameService.Setup(s => s.GetGameByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Game?)null);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SettleBetAsync(betId));
            #endregion
        }

        [Fact]
        public async Task SettleBetAsync_PlayerLoses_SetsPayoutZero()
        {
            #region Arrange
            var betId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var bet = new Bet { Id = betId, StatusId = (int)BetStatusEnum.Created, Amount = 50, WalletId = walletId, GameId = gameId };
            var game = new Game { Id = gameId };
            _betRepo.Setup(r => r.GetByIdAsync(betId)).ReturnsAsync(bet);
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);
            var service = CreateService();
            #endregion

            #region Act
            BetDTO? result = null;
            for (int i = 0; i < 10; i++)
            {
                _betRepo.Invocations.Clear();
                _betRepo.Setup(r => r.UpdateAsync(It.IsAny<Bet>())).ReturnsAsync(bet);
                _mapper.Setup(m => m.Map<BetDTO>(It.IsAny<Bet>())).Returns(new BetDTO { Id = betId, StatusId = (int)BetStatusEnum.Settled, Payout = 0 });
                result = await service.SettleBetAsync(betId);
                if (result.Payout == 0)
                    break;
            }
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Payout);
            Assert.Equal((int)BetStatusEnum.Settled, result.StatusId);
            #endregion
        }

        [Fact]
        public async Task SettleBetAsync_PlayerWins_CreditsWalletAndSetsPayout()
        {
            #region Arrange
            var betId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var bet = new Bet { Id = betId, StatusId = (int)BetStatusEnum.Created, Amount = 50, WalletId = walletId, GameId = gameId };
            var game = new Game { Id = gameId };
            var wallet = new Wallet { Id = walletId };
            _betRepo.Setup(r => r.GetByIdAsync(betId)).ReturnsAsync(bet);
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);
            _walletService.Setup(s => s.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);
            _walletService.Setup(s => s.CreditWalletAsync(wallet, 100, betId)).ReturnsAsync(new WalletTransaction());
            _betRepo.Setup(r => r.UpdateAsync(It.IsAny<Bet>())).ReturnsAsync(bet);
            _mapper.Setup(m => m.Map<BetDTO>(It.IsAny<Bet>())).Returns(new BetDTO { Id = betId, StatusId = (int)BetStatusEnum.Settled, Payout = 100 });

            var service = CreateService();
            #endregion

            #region Act
            BetDTO? result = null;
            for (int i = 0; i < 10; i++)
            {
                result = await service.SettleBetAsync(betId);
                if (result.Payout == 100)
                    break;
            }
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Payout);
            Assert.Equal((int)BetStatusEnum.Settled, result.StatusId);
            #endregion
        }
    }
}

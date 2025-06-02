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
using Application.Exceptions;

using BetStatusEnum = Domain.Enums.BetStatus;

namespace Tests.Services
{
    public class BetServiceTests
    {
        #region Fields and Dependencies

        private readonly Mock<IBetRepository> _betRepo = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<IWalletService> _walletService = new();
        private readonly Mock<IWalletTransactionService> _walletTxService = new();
        private readonly Mock<IGameService> _gameService = new();
        private readonly Mock<ILogger<BetService>> _logger = new();
        private readonly Mock<IUserNotificationService> _userNotificationService = new();
        private readonly Mock<IRandomService> _randomService = new();

        #endregion

        #region Helper Methods

        private BetService CreateService() =>
            new BetService(
                _betRepo.Object,
                _mapper.Object,
                _walletService.Object,
                _walletTxService.Object,
                _gameService.Object,
                _userNotificationService.Object,
                _randomService.Object,
                _logger.Object);

        #endregion

        #region PlaceBetAsync Tests

        #region Valid Scenarios

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
            _walletService.Setup(s => s.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);
            _walletService.Setup(s => s.WalletExistsAsync(walletId)).ReturnsAsync(true);
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);
            _mapper.Setup(m => m.Map<Bet>(betDTO)).Returns(betEntity);
            _betRepo.Setup(r => r.AddAsync(It.IsAny<Bet>())).ReturnsAsync(newBetEntity);
            _walletService.Setup(s => s.DebitWalletAsync(wallet, betDTO.Amount, betId))
                          .ReturnsAsync(new WalletTransaction { WalletId = walletId, Amount = betDTO.Amount });

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

        #endregion

        #region Error Scenarios

        [Fact]
        public async Task PlaceBetAsync_WalletNotFound_ThrowsWalletNotFoundException()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var betDTO = new PlaceBetDTO { WalletId = walletId, Amount = 10, CurrencyId = 1, GameId = Guid.NewGuid() };
            _walletService.Setup(s => s.WalletExistsAsync(walletId)).ReturnsAsync(false);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<WalletNotFoundException>(() => service.PlaceBetAsync(betDTO));
            #endregion
        }

        [Fact]
        public async Task PlaceBetAsync_PlayerNotFound_ThrowsPlayerNotFoundException()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var betDTO = new PlaceBetDTO { WalletId = walletId, Amount = 10, CurrencyId = 1, GameId = Guid.NewGuid() };
            _walletService.Setup(s => s.WalletExistsAsync(walletId)).ReturnsAsync(true);
            _walletService.Setup(s => s.GetPlayerByWalletIdAsync(walletId)).ReturnsAsync((Guid?)null);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<PlayerNotFoundException>(() => service.PlaceBetAsync(betDTO));
            #endregion
        }

        [Fact]
        public async Task PlaceBetAsync_InvalidCurrency_ThrowsInvalidCurrencyException()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var betDTO = new PlaceBetDTO { WalletId = walletId, Amount = 20, CurrencyId = -2, GameId = gameId };
            var wallet = new Wallet { Id = walletId, Balance = 100, CurrencyId = -2 };
            var game = new Game { Id = gameId, MinimalBetAmount = 10, MinimalBetCurrencyId = 1 };
            _walletService.Setup(s => s.WalletExistsAsync(walletId)).ReturnsAsync(true);
            _walletService.Setup(s => s.GetPlayerByWalletIdAsync(walletId)).ReturnsAsync(playerId);
            _walletService.Setup(s => s.GetWalletsByPlayerIdAsync(playerId)).ReturnsAsync(new List<Wallet> { wallet });
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidCurrencyException>(() => service.PlaceBetAsync(betDTO));
            #endregion
        }

        [Fact]
        public async Task PlaceBetAsync_InsufficientBalance_ThrowsInsufficientBalanceException()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var betDTO = new PlaceBetDTO { WalletId = walletId, Amount = 200, CurrencyId = 1, GameId = gameId };
            var wallet = new Wallet { Id = walletId, Balance = 100, CurrencyId = 1 };
            var game = new Game { Id = gameId, MinimalBetAmount = 10, MinimalBetCurrencyId = 1 };
            _walletService.Setup(s => s.WalletExistsAsync(walletId)).ReturnsAsync(true);
            _walletService.Setup(s => s.GetPlayerByWalletIdAsync(walletId)).ReturnsAsync(playerId);
            _walletService.Setup(s => s.GetWalletsByPlayerIdAsync(playerId)).ReturnsAsync(new List<Wallet> { wallet });
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InsufficientBalanceException>(() => service.PlaceBetAsync(betDTO));
            #endregion
        }

        [Fact]
        public async Task PlaceBetAsync_BetAmountBelowMinimum_ThrowsBetValidationException()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var betDTO = new PlaceBetDTO { WalletId = walletId, Amount = 5, CurrencyId = 1, GameId = gameId };
            var wallet = new Wallet { Id = walletId, Balance = 100, CurrencyId = 1 };
            var game = new Game { Id = gameId, MinimalBetAmount = 10, MinimalBetCurrencyId = 1 };
            _walletService.Setup(s => s.WalletExistsAsync(walletId)).ReturnsAsync(true);
            _walletService.Setup(s => s.GetPlayerByWalletIdAsync(walletId)).ReturnsAsync(playerId);
            _walletService.Setup(s => s.GetWalletsByPlayerIdAsync(playerId)).ReturnsAsync(new List<Wallet> { wallet });
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<BetValidationException>(() => service.PlaceBetAsync(betDTO));
            #endregion
        }

        #endregion

        #endregion

        #region SettleBetAsync Tests

        #region Error Scenarios

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
        public async Task SettleBetAsync_MissingWallet_Throws()
        {
            #region Arrange
            var betId = Guid.NewGuid();
            var bet = new Bet { Id = betId, StatusId = (int)BetStatusEnum.Created, Amount = 50, WalletId = Guid.NewGuid(), GameId = Guid.NewGuid() };
            _betRepo.Setup(r => r.GetByIdAsync(betId)).ReturnsAsync(bet);
            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SettleBetAsync(betId));
            #endregion
        }

        #endregion

        #region Outcome Scenarios

        [Fact]
        public async Task SettleBetAsync_PlayerLoses_SetsPayoutZero()
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
            _betRepo.Setup(r => r.UpdateAsync(It.IsAny<Bet>())).ReturnsAsync(bet);
            _mapper.Setup(m => m.Map<BetDTO>(It.IsAny<Bet>())).Returns(new BetDTO { Id = betId, StatusId = (int)BetStatusEnum.Settled, Payout = 0 });
            _randomService.Setup(s => s.GetRandomBoolean()).Returns(false);

            var service = CreateService();
            #endregion

            #region Act
            var result = await service.SettleBetAsync(betId);
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
            var betAmount = 50m;
            var odds = 2.00m;
            var expectedPayout = betAmount / (1 / odds);

            var bet = new Bet { Id = betId, StatusId = (int)BetStatusEnum.Created, Amount = betAmount, WalletId = walletId, GameId = gameId };
            var game = new Game { Id = gameId, Odds = odds };
            var wallet = new Wallet { Id = walletId };
            var currency = new Currency { Id = 1, Code = "USD" };

            _betRepo.Setup(r => r.GetByIdAsync(betId)).ReturnsAsync(bet);
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);
            _walletService.Setup(s => s.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);
            _walletService.Setup(s => s.CreditWalletAsync(wallet, expectedPayout, betId)).ReturnsAsync(new WalletTransaction());
            _walletService.Setup(s => s.GetCurrencyByWalletIdAsync(walletId)).ReturnsAsync(currency);
            _betRepo.Setup(r => r.UpdateAsync(It.IsAny<Bet>())).ReturnsAsync(bet);
            _mapper.Setup(m => m.Map<BetDTO>(It.IsAny<Bet>())).Returns(new BetDTO { Id = betId, StatusId = (int)BetStatusEnum.Settled, Payout = expectedPayout });
            _randomService.Setup(s => s.GetRandomBoolean()).Returns(true);

            var service = CreateService();
            #endregion

            #region Act
            var result = await service.SettleBetAsync(betId);
            #endregion

            #region Assert
            Assert.NotNull(result);
            Assert.Equal(expectedPayout, result.Payout);
            Assert.Equal((int)BetStatusEnum.Settled, result.StatusId);
            #endregion
        }

        [Fact]
        public async Task SettleBetAsync_AwardsBonusAfterThresholdConsecutiveLosses()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var bets = new List<Bet>
            {
                new Bet { Id = Guid.NewGuid(), StatusId = (int)BetStatusEnum.Created, Amount = 50, WalletId = walletId, GameId = gameId },
                new Bet { Id = Guid.NewGuid(), StatusId = (int)BetStatusEnum.Created, Amount = 50, WalletId = walletId, GameId = gameId },
                new Bet { Id = Guid.NewGuid(), StatusId = (int)BetStatusEnum.Created, Amount = 50, WalletId = walletId, GameId = gameId }
            };
            var game = new Game { Id = gameId, ConsecutiveLossBonusThreshold = 3 };
            var wallet = new Wallet { Id = walletId };

            _betRepo.SetupSequence(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(bets[0])
                    .ReturnsAsync(bets[1])
                    .ReturnsAsync(bets[2]);
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);
            _walletService.Setup(s => s.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);
            _walletService.Setup(s => s.CreditWalletAsync(wallet, It.IsAny<decimal>(), It.IsAny<Guid>()))
                          .ReturnsAsync(new WalletTransaction());
            _randomService.SetupSequence(s => s.GetRandomBoolean())
                          .Returns(false) // 1st loss
                          .Returns(false) // 2nd loss
                          .Returns(false); // 3rd loss - should award bonus
            _walletService.Setup(s => s.GetBetsByWalletIdAsync(walletId)).ReturnsAsync(bets);

            var service = CreateService();
            #endregion

            #region Act
            foreach (var bet in bets)
            {
                await service.SettleBetAsync(bet.Id);
            }
            #endregion

            #region Assert
            // Verify that the bonus was credited after 3 consecutive losses
            _walletService.Verify(s => s.CreditWalletAsync(wallet, It.IsAny<decimal>(), It.IsAny<Guid>()), Times.Once);
            #endregion
        }

        [Fact]
        public async Task SettleBetAsync_NoBonusIfBelowThreshold()
        {
            #region Arrange
            var walletId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var bets = new List<Bet>
            {
                new Bet { Id = Guid.NewGuid(), StatusId = (int)BetStatusEnum.Created, Amount = 50, WalletId = walletId, GameId = gameId },
                new Bet { Id = Guid.NewGuid(), StatusId = (int)BetStatusEnum.Created, Amount = 50, WalletId = walletId, GameId = gameId }
            };
            var game = new Game { Id = gameId, ConsecutiveLossBonusThreshold = 3 };
            var wallet = new Wallet { Id = walletId };

            _betRepo.SetupSequence(r => r.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(bets[0])
                    .ReturnsAsync(bets[1]);
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);
            _walletService.Setup(s => s.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);
            _walletService.Setup(s => s.GetBetsByWalletIdAsync(walletId)).ReturnsAsync(bets);
            _randomService.SetupSequence(s => s.GetRandomBoolean())
                          .Returns(false) // 1st loss
                          .Returns(false); // 2nd loss - below threshold

            var service = CreateService();
            #endregion

            #region Act
            foreach (var bet in bets)
            {
                await service.SettleBetAsync(bet.Id);
            }
            #endregion

            #region Assert
            // Verify that no bonus was credited
            _walletService.Verify(s => s.CreditWalletAsync(It.IsAny<Wallet>(), It.IsAny<decimal>(), It.IsAny<Guid>()), Times.Never);
            #endregion
        }

        [Fact]
        public async Task SettleBetAsync_CorrectBonusAmountCalculation()
        {
            // Arrange
            var walletId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var betAmount = 100m;
            var bonusPercentage = 0.25m; // 25% bonus

            var bet = new Bet {
                Id = Guid.NewGuid(),
                StatusId = (int)BetStatusEnum.Created,
                Amount = betAmount,
                WalletId = walletId,
                GameId = gameId
            };
            var game = new Game {
                Id = gameId,
                ConsecutiveLossBonusThreshold = 1,
                ConsecutiveLossBonusPercentage = bonusPercentage
            };
            var wallet = new Wallet { Id = walletId };
            var expectedBonusAmount = Math.Round(betAmount * bonusPercentage, 2); // 25.00

            _betRepo.Setup(r => r.GetByIdAsync(bet.Id)).ReturnsAsync(bet);
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);
            _walletService.Setup(s => s.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);
            _walletService.Setup(s => s.GetBetsByWalletIdAsync(walletId))
                         .ReturnsAsync(new List<Bet> { bet });
            _randomService.Setup(s => s.GetRandomBoolean()).Returns(false);

            var service = CreateService();

            // Act
            await service.SettleBetAsync(bet.Id);

            // Assert
            _walletService.Verify(s => s.CreditWalletAsync(
                wallet,
                expectedBonusAmount,
                It.IsAny<Guid>()
            ), Times.Once);
        }

        [Fact]
        public async Task SettleBetAsync_ZeroBonusPercentage_NoBonus()
        {
            // Arrange
            var walletId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var bet = new Bet {
                Id = Guid.NewGuid(),
                StatusId = (int)BetStatusEnum.Created,
                Amount = 100,
                WalletId = walletId,
                GameId = gameId
            };
            var game = new Game {
                Id = gameId,
                ConsecutiveLossBonusThreshold = 3,
                ConsecutiveLossBonusPercentage = 0 // 0% bonus
            };
            var wallet = new Wallet { Id = walletId };

            _betRepo.Setup(r => r.GetByIdAsync(bet.Id)).ReturnsAsync(bet);
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);
            _walletService.Setup(s => s.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);
            _walletService.Setup(s => s.GetBetsByWalletIdAsync(walletId))
                         .ReturnsAsync(new List<Bet> { bet });
            _randomService.Setup(s => s.GetRandomBoolean()).Returns(false);

            var service = CreateService();

            // Act
            await service.SettleBetAsync(bet.Id);

            // Assert
            _walletService.Verify(s => s.CreditWalletAsync(
                It.IsAny<Wallet>(),
                It.IsAny<decimal>(),
                It.IsAny<Guid>()
            ), Times.Never);
        }

        #endregion

        #endregion

        #region CancelBetAsync Tests

        #region Error Scenarios

        [Fact]
        public async Task CancelBetAsync_BetNotFound_Throws()
        {
            #region Arrange
            var betId = Guid.NewGuid();
            _betRepo.Setup(r => r.GetByIdAsync(betId)).ReturnsAsync((Bet?)null);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CancelBetAsync(betId, "Bet not found."));
            #endregion
        }

        [Fact]
        public async Task CancelBetAsync_InvalidBetStatus_Throws()
        {
            #region Arrange
            var betId = Guid.NewGuid();
            var bet = new Bet { Id = betId, StatusId = (int)BetStatusEnum.Settled };
            _betRepo.Setup(r => r.GetByIdAsync(betId)).ReturnsAsync(bet);

            var service = CreateService();
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CancelBetAsync(betId, "Invalid status."));
            #endregion
        }

        #endregion

        #region Valid Scenarios

        [Fact]
        public async Task CancelBetAsync_SuccessfulCancellation_RefundsWalletAndUpdatesBet()
        {
            #region Arrange
            var betId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var bet = new Bet { Id = betId, StatusId = (int)BetStatusEnum.Created, Amount = 100, WalletId = walletId, GameId = gameId };
            var wallet = new Wallet { Id = walletId, Balance = 200 };
            var game = new Game { Id = gameId, CancelTaxPercentage = 10 };

            _betRepo.Setup(r => r.GetByIdAsync(betId)).ReturnsAsync(bet);
            _walletService.Setup(s => s.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);
            _walletService.Setup(s => s.CreditWalletAsync(wallet, 90, betId)).ReturnsAsync(new WalletTransaction());
            _betRepo.Setup(r => r.UpdateAsync(It.IsAny<Bet>())).ReturnsAsync(bet);

            var service = CreateService();
            #endregion

            #region Act
            await service.CancelBetAsync(betId, "User requested cancellation.");
            #endregion

            #region Assert
            _walletService.Verify(s => s.CreditWalletAsync(wallet, 90, betId), Times.Once);
            _betRepo.Verify(r => r.UpdateAsync(It.Is<Bet>(b => b.StatusId == (int)BetStatusEnum.Cancelled && b.Note == "User requested cancellation.")), Times.Once);
            #endregion
        }

        [Fact]
        public async Task CancelBetAsync_RefundAmountZero_DoesNotCreditWallet()
        {
            #region Arrange
            var betId = Guid.NewGuid();
            var walletId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var bet = new Bet { Id = betId, StatusId = (int)BetStatusEnum.Created, Amount = 100, WalletId = walletId, GameId = gameId };
            var wallet = new Wallet { Id = walletId, Balance = 200 };
            var game = new Game { Id = gameId, CancelTaxPercentage = 100 };

            _betRepo.Setup(r => r.GetByIdAsync(betId)).ReturnsAsync(bet);
            _walletService.Setup(s => s.GetWalletByIdAsync(walletId)).ReturnsAsync(wallet);
            _gameService.Setup(s => s.GetGameByIdAsync(gameId)).ReturnsAsync(game);
            _betRepo.Setup(r => r.UpdateAsync(It.IsAny<Bet>())).ReturnsAsync(bet);

            var service = CreateService();
            #endregion

            #region Act
            await service.CancelBetAsync(betId, "High tax percentage.");
            #endregion

            #region Assert
            _walletService.Verify(s => s.CreditWalletAsync(It.IsAny<Wallet>(), It.IsAny<decimal>(), It.IsAny<Guid>()), Times.Never);
            _betRepo.Verify(r => r.UpdateAsync(It.Is<Bet>(b => b.StatusId == (int)BetStatusEnum.Cancelled)), Times.Once);
            #endregion
        }

        #endregion

        #endregion
    }
}

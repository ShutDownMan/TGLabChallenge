using Xunit;
using Moq;
using Application.Services;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Application.Exceptions;
using System;
using System.Threading.Tasks;
using Application.Models;
using FluentValidation;
using FluentValidation.Results;

namespace Tests.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsToken()
        {
            #region Arrange
            var playerService = new Mock<IPlayerService>();
            var currencyService = new Mock<ICurrencyService>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            var walletRepo = new Mock<IWalletRepository>();
            var walletTxService = new Mock<IWalletTransactionService>();
            var user = new Player { Id = Guid.NewGuid(), Username = "test", Email = "test@email.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass") };
            playerService.Setup(s => s.GetByUsernameAsync("test")).ReturnsAsync(user);
            jwtGen.Setup(j => j.GenerateToken(user)).Returns("token");
            var service = new AuthService(playerService.Object, currencyService.Object, jwtGen.Object, walletRepo.Object, walletTxService.Object);
            #endregion

            #region Act
            var token = await service.LoginAsync("test", "pass");
            #endregion

            #region Assert
            Assert.Equal("token", token);
            #endregion
        }

        [Fact]
        public async Task LoginAsync_WithValidEmail_ReturnsToken()
        {
            #region Arrange
            var playerService = new Mock<IPlayerService>();
            var currencyService = new Mock<ICurrencyService>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            var walletRepo = new Mock<IWalletRepository>();
            var walletTxService = new Mock<IWalletTransactionService>();
            var user = new Player { Id = Guid.NewGuid(), Username = "test", Email = "test@email.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass") };
            playerService.Setup(s => s.GetByUsernameAsync("test@email.com")).ReturnsAsync((Player?)null);
            playerService.Setup(s => s.GetByEmailAsync("test@email.com")).ReturnsAsync(user);
            jwtGen.Setup(j => j.GenerateToken(user)).Returns("token");
            var service = new AuthService(playerService.Object, currencyService.Object, jwtGen.Object, walletRepo.Object, walletTxService.Object);
            #endregion

            #region Act
            var token = await service.LoginAsync("test@email.com", "pass");
            #endregion

            #region Assert
            Assert.Equal("token", token);
            #endregion
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ThrowsUnauthorized()
        {
            #region Arrange
            var playerService = new Mock<IPlayerService>();
            var currencyService = new Mock<ICurrencyService>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            var walletRepo = new Mock<IWalletRepository>();
            var walletTxService = new Mock<IWalletTransactionService>();
            playerService.Setup(s => s.GetByUsernameAsync("test")).ReturnsAsync((Player?)null);
            playerService.Setup(s => s.GetByEmailAsync("test")).ReturnsAsync((Player?)null);
            var service = new AuthService(playerService.Object, currencyService.Object, jwtGen.Object, walletRepo.Object, walletTxService.Object);
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync("test", "wrong"));
            #endregion
        }

        [Fact]
        public async Task RegisterAsync_WithExistingUsername_ThrowsException()
        {
            #region Arrange
            var playerService = new Mock<IPlayerService>();
            var currencyService = new Mock<ICurrencyService>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            var walletRepo = new Mock<IWalletRepository>();
            var walletTxService = new Mock<IWalletTransactionService>();
            playerService.Setup(s => s.UsernameExistsAsync("test")).ReturnsAsync(true);
            var service = new AuthService(playerService.Object, currencyService.Object, jwtGen.Object, walletRepo.Object, walletTxService.Object);
            #endregion

            #region Act & Assert
            await Assert.ThrowsAsync<UserAlreadyExistsException>(() => service.RegisterAsync("test", "pass", "test@email.com", 1, 1));
            #endregion
        }

        [Fact]
        public async Task RegisterAsync_WithNewUsername_AddsUserWithHashedPassword()
        {
            #region Arrange
            var playerService = new Mock<IPlayerService>();
            var currencyService = new Mock<ICurrencyService>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            var walletRepo = new Mock<IWalletRepository>();
            var walletTxService = new Mock<IWalletTransactionService>();
            playerService.Setup(s => s.UsernameExistsAsync("newuser")).ReturnsAsync(false);
            playerService.Setup(s => s.EmailExistsAsync("new@email.com")).ReturnsAsync(false);
            Player? addedPlayer = null;
            playerService.Setup(s => s.AddAsync(It.IsAny<Player>()))
                .Callback<Player>(p => addedPlayer = p)
                .Returns(Task.CompletedTask);

            // Setup currencyRepo to accept any currencyId
            currencyService.Setup(s => s.CurrencyExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            currencyService.Setup(s => s.GetDefaultCurrencyAsync()).ReturnsAsync(new Currency { Id = 1 });

            // Setup walletRepo to accept any wallet
            walletRepo.Setup(r => r.AddAsync(It.IsAny<Wallet>())).Returns(Task.CompletedTask);

            // Setup walletTxService to accept any checkpoint
            walletTxService.Setup(s => s.CheckpointWalletAsync(It.IsAny<Wallet>(), It.IsAny<decimal>(), null))
                .ReturnsAsync(new WalletTransaction());

            var service = new AuthService(playerService.Object, currencyService.Object, jwtGen.Object, walletRepo.Object, walletTxService.Object);
            #endregion

            #region Act
            await service.RegisterAsync("newuser", "newpass", "new@email.com", 1, 0);
            #endregion

            #region Assert
            Assert.NotNull(addedPlayer);
            Assert.Equal("newuser", addedPlayer.Username);
            Assert.Equal("new@email.com", addedPlayer.Email);
            Assert.NotNull(addedPlayer.PasswordHash);
            Assert.NotEqual("newpass", addedPlayer.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("newpass", addedPlayer.PasswordHash));
            #endregion
        }

        [Fact]
        public void RegisterRequestValidator_InvalidData_ReturnsValidationErrors()
        {
            #region Arrange
            var validator = new RegisterRequestValidator();
            var invalidRequest = new RegisterRequest
            {
                Username = "",
                Password = "",
                Email = "not-an-email",
                InitialBalance = -10
            };
            #endregion

            #region Act
            ValidationResult result = validator.Validate(invalidRequest);
            #endregion

            #region Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Username");
            Assert.Contains(result.Errors, e => e.PropertyName == "Password");
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorCode == "EMAIL_INVALID");
            Assert.Contains(result.Errors, e => e.PropertyName == "InitialBalance");
            #endregion
        }

        [Fact]
        public void RegisterRequestValidator_ValidData_PassesValidation()
        {
            #region Arrange
            var validator = new RegisterRequestValidator();
            var validRequest = new RegisterRequest
            {
                Username = "user1",
                Password = "pass123",
                Email = "user1@email.com",
                InitialBalance = 0
            };
            #endregion

            #region Act
            ValidationResult result = validator.Validate(validRequest);
            #endregion

            #region Assert
            Assert.True(result.IsValid);
            #endregion
        }

        [Fact]
        public void LoginRequestValidator_InvalidData_ReturnsValidationErrors()
        {
            #region Arrange
            var validator = new LoginRequestValidator();
            var invalidRequest = new LoginRequest("", "");
            #endregion

            #region Act
            ValidationResult result = validator.Validate(invalidRequest);
            #endregion

            #region Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Identifier");
            Assert.Contains(result.Errors, e => e.PropertyName == "Password");
            #endregion
        }

        [Fact]
        public void LoginRequestValidator_ValidData_PassesValidation()
        {
            #region Arrange
            var validator = new LoginRequestValidator();
            var validRequest = new LoginRequest("user1", "pass123");
            #endregion

            #region Act
            ValidationResult result = validator.Validate(validRequest);
            #endregion

            #region Assert
            Assert.True(result.IsValid);
            #endregion
        }
    }
}

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
            // Arrange
            var userRepo = new Mock<IPlayerRepository>();
            var currencyRepo = new Mock<ICurrencyRepository>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            var walletRepo = new Mock<IWalletRepository>();
            var user = new Player { Id = Guid.NewGuid(), Username = "test", Email = "test@email.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass") };
            userRepo.Setup(r => r.GetByUsernameAsync("test")).ReturnsAsync(user);
            jwtGen.Setup(j => j.GenerateToken(user)).Returns("token");
            var service = new AuthService(userRepo.Object, currencyRepo.Object, jwtGen.Object, walletRepo.Object);

            // Act
            var token = await service.LoginAsync("test", "pass");

            // Assert
            Assert.Equal("token", token);
        }

        [Fact]
        public async Task LoginAsync_WithValidEmail_ReturnsToken()
        {
            // Arrange
            var userRepo = new Mock<IPlayerRepository>();
            var currencyRepo = new Mock<ICurrencyRepository>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            var walletRepo = new Mock<IWalletRepository>();
            var user = new Player { Id = Guid.NewGuid(), Username = "test", Email = "test@email.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass") };
            userRepo.Setup(r => r.GetByUsernameAsync("test@email.com")).ReturnsAsync((Player?)null);
            userRepo.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(user);
            jwtGen.Setup(j => j.GenerateToken(user)).Returns("token");
            var service = new AuthService(userRepo.Object, currencyRepo.Object, jwtGen.Object, walletRepo.Object);

            // Act
            var token = await service.LoginAsync("test@email.com", "pass");

            // Assert
            Assert.Equal("token", token);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ThrowsUnauthorized()
        {
            var userRepo = new Mock<IPlayerRepository>();
            var currencyRepo = new Mock<ICurrencyRepository>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            var walletRepo = new Mock<IWalletRepository>();
            userRepo.Setup(r => r.GetByUsernameAsync("test")).ReturnsAsync((Player?)null);
            userRepo.Setup(r => r.GetByEmailAsync("test")).ReturnsAsync((Player?)null);
            var service = new AuthService(userRepo.Object, currencyRepo.Object, jwtGen.Object, walletRepo.Object);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync("test", "wrong"));
        }

        [Fact]
        public async Task RegisterAsync_WithExistingUsername_ThrowsException()
        {
            var userRepo = new Mock<IPlayerRepository>();
            var currencyRepo = new Mock<ICurrencyRepository>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            var walletRepo = new Mock<IWalletRepository>();
            userRepo.Setup(r => r.UsernameExistsAsync("test")).ReturnsAsync(true);
            var service = new AuthService(userRepo.Object, currencyRepo.Object, jwtGen.Object, walletRepo.Object);

            await Assert.ThrowsAsync<UserAlreadyExistsException>(() => service.RegisterAsync("test", "pass", "test@email.com", 1, 1));
        }

        [Fact]
        public async Task RegisterAsync_WithNewUsername_AddsUserWithHashedPassword()
        {
            // Arrange
            var userRepo = new Mock<IPlayerRepository>();
            var currencyRepo = new Mock<ICurrencyRepository>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            var walletRepo = new Mock<IWalletRepository>();
            userRepo.Setup(r => r.UsernameExistsAsync("newuser")).ReturnsAsync(false);
            Player? addedPlayer = null;
            userRepo.Setup(r => r.AddAsync(It.IsAny<Player>()))
                .Callback<Player>(p => addedPlayer = p)
                .Returns(Task.CompletedTask);

            // Setup currencyRepo to accept any currencyId
            currencyRepo.Setup(r => r.CurrencyExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            // Setup walletRepo to accept any wallet
            walletRepo.Setup(r => r.AddAsync(It.IsAny<Wallet>())).Returns(Task.CompletedTask);

            var service = new AuthService(userRepo.Object, currencyRepo.Object, jwtGen.Object, walletRepo.Object);

            // Act
            await service.RegisterAsync("newuser", "newpass", "new@email.com", 1, 0);

            // Assert
            Assert.NotNull(addedPlayer);
            Assert.Equal("newuser", addedPlayer.Username);
            Assert.Equal("new@email.com", addedPlayer.Email);
            Assert.NotNull(addedPlayer.PasswordHash);
            Assert.NotEqual("newpass", addedPlayer.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify("newpass", addedPlayer.PasswordHash));
        }

        [Fact]
        public void RegisterRequestValidator_InvalidData_ReturnsValidationErrors()
        {
            var validator = new RegisterRequestValidator();
            var invalidRequest = new RegisterRequest
            {
                Username = "",
                Password = "",
                Email = "not-an-email",
                InitialBalance = -10
            };

            ValidationResult result = validator.Validate(invalidRequest);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Username");
            Assert.Contains(result.Errors, e => e.PropertyName == "Password");
            Assert.Contains(result.Errors, e => e.PropertyName == "Email" && e.ErrorCode == "EMAIL_INVALID");
            Assert.Contains(result.Errors, e => e.PropertyName == "InitialBalance");
        }

        [Fact]
        public void RegisterRequestValidator_ValidData_PassesValidation()
        {
            var validator = new RegisterRequestValidator();
            var validRequest = new RegisterRequest
            {
                Username = "user1",
                Password = "pass123",
                Email = "user1@email.com",
                InitialBalance = 0
            };

            ValidationResult result = validator.Validate(validRequest);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void LoginRequestValidator_InvalidData_ReturnsValidationErrors()
        {
            var validator = new LoginRequestValidator();
            var invalidRequest = new LoginRequest("", "");

            ValidationResult result = validator.Validate(invalidRequest);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Identifier");
            Assert.Contains(result.Errors, e => e.PropertyName == "Password");
        }

        [Fact]
        public void LoginRequestValidator_ValidData_PassesValidation()
        {
            var validator = new LoginRequestValidator();
            var validRequest = new LoginRequest("user1", "pass123");

            ValidationResult result = validator.Validate(validRequest);

            Assert.True(result.IsValid);
        }
    }
}

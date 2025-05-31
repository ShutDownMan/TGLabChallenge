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
            var user = new Player { Id = Guid.NewGuid(), Username = "test", Email = "test@email.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass") };
            userRepo.Setup(r => r.GetByUsernameAsync("test")).ReturnsAsync(user);
            jwtGen.Setup(j => j.GenerateToken(user)).Returns("token");
            var service = new AuthService(userRepo.Object, currencyRepo.Object, jwtGen.Object);

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
            var user = new Player { Id = Guid.NewGuid(), Username = "test", Email = "test@email.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass") };
            userRepo.Setup(r => r.GetByUsernameAsync("test@email.com")).ReturnsAsync((Player?)null);
            userRepo.Setup(r => r.GetByEmailAsync("test@email.com")).ReturnsAsync(user);
            jwtGen.Setup(j => j.GenerateToken(user)).Returns("token");
            var service = new AuthService(userRepo.Object, currencyRepo.Object, jwtGen.Object);

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
            userRepo.Setup(r => r.GetByUsernameAsync("test")).ReturnsAsync((Player?)null);
            userRepo.Setup(r => r.GetByEmailAsync("test")).ReturnsAsync((Player?)null);
            var service = new AuthService(userRepo.Object, currencyRepo.Object, jwtGen.Object);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync("test", "wrong"));
        }

        [Fact]
        public async Task RegisterAsync_WithExistingUsername_ThrowsException()
        {
            var userRepo = new Mock<IPlayerRepository>();
            var currencyRepo = new Mock<ICurrencyRepository>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            userRepo.Setup(r => r.UsernameExistsAsync("test")).ReturnsAsync(true);
            var service = new AuthService(userRepo.Object, currencyRepo.Object, jwtGen.Object);

            await Assert.ThrowsAsync<UserAlreadyExistsException>(() => service.RegisterAsync("test", "pass", "test@email.com", 1, 1));
        }

        [Fact]
        public async Task RegisterAsync_WithNewUsername_AddsUserWithHashedPassword()
        {
            // Arrange
            var userRepo = new Mock<IPlayerRepository>();
            var currencyRepo = new Mock<ICurrencyRepository>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            userRepo.Setup(r => r.UsernameExistsAsync("newuser")).ReturnsAsync(false);
            Player? addedPlayer = null;
            userRepo.Setup(r => r.AddAsync(It.IsAny<Player>()))
                .Callback<Player>(p => addedPlayer = p)
                .Returns(Task.CompletedTask);

            // Setup currencyRepo to accept any currencyId
            currencyRepo.Setup(r => r.CurrencyExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var service = new AuthService(userRepo.Object, currencyRepo.Object, jwtGen.Object);

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
    }
}

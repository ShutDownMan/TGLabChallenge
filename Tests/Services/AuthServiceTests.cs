using Xunit;
using Moq;
using Application.Services;
using Application.Interfaces;
using Domain.Entities;
using Domain.Security;
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
            var userRepo = new Mock<IUserRepository>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            var user = new User { Id = Guid.NewGuid(), Username = "test", PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass") };
            userRepo.Setup(r => r.GetByUsernameAsync("test")).ReturnsAsync(user);
            jwtGen.Setup(j => j.GenerateToken(user)).Returns("token");
            var service = new AuthService(userRepo.Object, jwtGen.Object);

            // Act
            var token = await service.LoginAsync("test", "pass");

            // Assert
            Assert.Equal("token", token);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ThrowsUnauthorized()
        {
            var userRepo = new Mock<IUserRepository>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            userRepo.Setup(r => r.GetByUsernameAsync("test")).ReturnsAsync((User?)null);
            var service = new AuthService(userRepo.Object, jwtGen.Object);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.LoginAsync("test", "wrong"));
        }

        [Fact]
        public async Task RegisterAsync_WithExistingUsername_ThrowsException()
        {
            var userRepo = new Mock<IUserRepository>();
            var jwtGen = new Mock<IJwtTokenGenerator>();
            userRepo.Setup(r => r.UsernameExistsAsync("test")).ReturnsAsync(true);
            var service = new AuthService(userRepo.Object, jwtGen.Object);

            await Assert.ThrowsAsync<UserAlreadyExistsException>(() => service.RegisterAsync("test", "pass"));
        }
    }
}

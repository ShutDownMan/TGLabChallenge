using Xunit;
using Moq;
using Application.Services;
using Application.Interfaces;
using Domain.Entities;
using Application.Models;
using System;
using System.Threading.Tasks;

namespace Tests.Services
{
    public class UserServiceTests
    {
        [Fact]
        public async Task GetProfileAsync_WithExistingUser_ReturnsProfile()
        {
            var userRepo = new Mock<IUserRepository>();
            var user = new Player { Id = Guid.NewGuid(), Username = "test" };
            userRepo.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
            var service = new UserService(userRepo.Object);

            var profile = await service.GetProfileAsync(user.Id);

            Assert.NotNull(profile);
            Assert.Equal("test", profile!.Username);
        }

        [Fact]
        public async Task GetProfileAsync_WithNonExistingUser_ReturnsNull()
        {
            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Player?)null!);
            var service = new UserService(userRepo.Object);

            var profile = await service.GetProfileAsync(Guid.NewGuid());

            Assert.Null(profile);
        }
    }
}

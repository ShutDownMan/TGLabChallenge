using Xunit;
using Moq;
using Application.Services;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Application.Models;
using System;
using System.Threading.Tasks;

namespace Tests.Services
{
    public class PlayerServiceTests
    {
        [Fact]
        public async Task GetProfileAsync_WithExistingPlayer_ReturnsProfile()
        {
            var playerRepo = new Mock<IPlayerRepository>();
            var player = new Player { Id = Guid.NewGuid(), Username = "test" };
            playerRepo.Setup(r => r.GetByIdAsync(player.Id)).ReturnsAsync(player);
            var service = new PlayerService(playerRepo.Object);

            var profile = await service.GetProfileAsync(player.Id);

            Assert.NotNull(profile);
            Assert.Equal("test", profile!.Username);
        }

        [Fact]
        public async Task GetProfileAsync_WithNonExistingPlayer_ReturnsNull()
        {
            var playerRepo = new Mock<IPlayerRepository>();
            playerRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Player?)null!);
            var service = new PlayerService(playerRepo.Object);

            var profile = await service.GetProfileAsync(Guid.NewGuid());

            Assert.Null(profile);
        }
    }
}

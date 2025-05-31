using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        public PlayerService(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<PlayerProfileDto?> GetProfileAsync(Guid playerId)
        {
            var player = await _playerRepository.GetByIdAsync(playerId);
            if (player == null) return null;

            return new PlayerProfileDto
            {
                Id = player.Id,
                Username = player.Username,
                Email = player.Email,
                CreatedAt = player.CreatedAt
            };
        }
    }
}

using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _userRepository;
        public PlayerService(IPlayerRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<PlayerProfileDto?> GetProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            return new PlayerProfileDto
            {
                Username = user.Username
                // Map other fields as needed
            };
        }
    }
}

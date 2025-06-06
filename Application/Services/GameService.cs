using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Application.Services
{
    /// <summary>
    /// Service responsible for handling game-related operations.
    /// </summary>
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameService"/> class.
        /// </summary>
        /// <param name="gameRepository">The repository for accessing game data.</param>
        public GameService(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        /// <summary>
        /// Retrieves a game by its unique identifier.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game.</param>
        /// <returns>The game entity if found; otherwise, null.</returns>
        public async Task<Game?> GetGameByIdAsync(Guid gameId)
        {
            return await _gameRepository.GetByIdAsync(gameId);
        }

        /// <summary>
        /// Retrieves the consecutive loss bonus threshold for a game.
        /// </summary>
        /// <param name="gameId">The unique identifier of the game.</param>
        /// <returns>The consecutive loss bonus threshold, or null if not set.</returns>
        public async Task<int?> GetConsecutiveLossBonusThresholdAsync(Guid gameId)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
                throw new KeyNotFoundException($"Game with ID {gameId} not found.");
            return game.ConsecutiveLossBonusThreshold;
        }
    }
}

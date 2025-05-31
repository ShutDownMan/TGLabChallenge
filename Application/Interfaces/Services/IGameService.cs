using Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IGameService
    {
        Task<Game?> GetGameByIdAsync(Guid gameId);
    }
}

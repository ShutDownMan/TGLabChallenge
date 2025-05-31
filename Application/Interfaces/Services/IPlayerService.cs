using System.Threading.Tasks;
using Application.Models;

namespace Application.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<PlayerProfileDTO?> GetProfileAsync(Guid playerId);
    }
}
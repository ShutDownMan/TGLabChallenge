using System.Threading.Tasks;
using Application.Models;

namespace Application.Interfaces
{
    public interface IPlayerService
    {
        Task<PlayerProfileDto?> GetProfileAsync(Guid userId);
    }
}

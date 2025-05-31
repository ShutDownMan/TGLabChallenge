using System.Threading.Tasks;
using Application.Models;

namespace Application.Interfaces.Services
{
    public interface IPlayerService
    {
        Task<PlayerProfileDto?> GetProfileAsync(Guid userId);
    }
}
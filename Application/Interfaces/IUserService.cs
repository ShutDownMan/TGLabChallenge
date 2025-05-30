using System.Threading.Tasks;
using Application.Models;

namespace Application.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto?> GetProfileAsync(Guid userId);
    }
}

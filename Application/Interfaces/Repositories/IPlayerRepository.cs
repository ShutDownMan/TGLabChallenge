using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IPlayerRepository
    {
        Task<Player?> GetByUsernameAsync(string username);
        Task<Player?> GetByEmailAsync(string email);
        Task<Player> GetByIdAsync(Guid id);
        Task AddAsync(Player user);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
    }

}
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User> GetByIdAsync(Guid id);
        Task AddAsync(User user);
        Task<bool> UsernameExistsAsync(string username);
    }

}

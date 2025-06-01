using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IBetRepository
    {
        Task<Bet?> GetByIdAsync(Guid id);
        Task<IEnumerable<Bet>> GetByUserAsync(Guid userId);
        Task<Bet> AddAsync(Bet bet);
        Task<Bet> UpdateAsync(Bet bet);
        Task CancelAsync(Guid betId);
    }

}
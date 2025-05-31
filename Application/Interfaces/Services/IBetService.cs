using Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IBetService
    {
        Task<BetDto> PlaceBetAsync(BetDto betDto);
        Task CancelBetAsync(BetDto betDto);
        Task<BetDto?> GetBetByIdAsync(Guid id);
        Task<IEnumerable<BetDto>> GetBetsByUserAsync(Guid userId);
    }
}
using Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IBetService
    {
        Task<BetDTO> PlaceBetAsync(PlaceBetDTO betDto);
        Task CancelBetAsync(Guid betId, string? cancelReason);
        Task<BetDTO?> GetBetByIdAsync(Guid id);
        Task<IEnumerable<BetDTO>> GetBetsByUserAsync(Guid userId);
    }
}
using Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IBetService
    {
        Task<PlaceBetDTO> PlaceBetAsync(PlaceBetDTO betDto);
        Task CancelBetAsync(PlaceBetDTO betDto);
        Task<PlaceBetDTO?> GetBetByIdAsync(Guid id);
        Task<IEnumerable<PlaceBetDTO>> GetBetsByUserAsync(Guid userId);
    }
}
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IWalletService
    {
        Task<List<Wallet>> GetWalletsByPlayerIdAsync(Guid playerId);
    }
}

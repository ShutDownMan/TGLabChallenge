using Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UserNotificationService<THub> : IUserNotificationService where THub : Hub
    {
        private readonly IHubContext<THub> _hubContext;

        public UserNotificationService(IHubContext<THub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyBetUpdateAsync(Guid playerId, object betDto)
        {
            await _hubContext.Clients.User(playerId.ToString())
                .SendAsync("bet-update", betDto);
        }

        public async Task NotifyBetCancelledAsync(Guid playerId, object betDto)
        {
            await _hubContext.Clients.User(playerId.ToString())
                .SendAsync("bet-cancelled", betDto);
        }

        public async Task NotifyBetSettledAsync(Guid playerId, object betDto)
        {
            await _hubContext.Clients.User(playerId.ToString())
                .SendAsync("bet-settled", betDto);
        }

        public async Task NotifyWalletTransactionUpdateAsync(Guid playerId, object transaction)
        {
            await _hubContext.Clients.User(playerId.ToString())
                .SendAsync("wallet-transaction-update", transaction);
        }
    }
}

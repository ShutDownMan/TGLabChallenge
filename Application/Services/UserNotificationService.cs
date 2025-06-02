using Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Application.Services
{
    /// <summary>
    /// Service responsible for sending notifications to users via SignalR.
    /// </summary>
    /// <typeparam name="THub">The type of SignalR hub used for notifications.</typeparam>
    public class UserNotificationService<THub> : IUserNotificationService where THub : Hub
    {
        private readonly IHubContext<THub> _hubContext;
        private readonly ILogger<UserNotificationService<THub>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotificationService{THub}"/> class.
        /// </summary>
        /// <param name="hubContext">The SignalR hub context for sending notifications.</param>
        /// <param name="logger">The logger for logging toast notifications.</param>
        public UserNotificationService(IHubContext<THub> hubContext, ILogger<UserNotificationService<THub>> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        private void ShowToastNotification(string message)
        {
            _logger.LogInformation($"Toast Notification: {message}");
        }

        /// <summary>
        /// Sends a notification to a user about a bet update.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <param name="betDto">The bet data transfer object containing updated information.</param>
        public async Task NotifyBetUpdateAsync(Guid playerId, object betDto)
        {
            await _hubContext.Clients.User(playerId.ToString())
                .SendAsync("bet-update", betDto);
            ShowToastNotification("Bet updated successfully.");
        }

        /// <summary>
        /// Sends a notification to a user about a bet cancellation.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <param name="betDto">The bet data transfer object containing cancellation information.</param>
        public async Task NotifyBetCancelledAsync(Guid playerId, object betDto)
        {
            await _hubContext.Clients.User(playerId.ToString())
                .SendAsync("bet-cancelled", betDto);
            ShowToastNotification("Bet cancelled successfully.");
        }

        /// <summary>
        /// Sends a notification to a user about a bet settlement.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <param name="betDto">The bet data transfer object containing settlement information.</param>
        public async Task NotifyBetSettledAsync(Guid playerId, object betDto)
        {
            await _hubContext.Clients.User(playerId.ToString())
                .SendAsync("bet-settled", betDto);
            ShowToastNotification("Bet settled successfully.");
        }

        /// <summary>
        /// Sends a notification to a user about a wallet transaction update.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <param name="transaction">The wallet transaction data transfer object containing updated information.</param>
        public async Task NotifyWalletTransactionUpdateAsync(Guid playerId, object transaction)
        {
            await _hubContext.Clients.User(playerId.ToString())
                .SendAsync("wallet-transaction-update", transaction);
            ShowToastNotification("Wallet transaction updated successfully.");
        }
    }
}

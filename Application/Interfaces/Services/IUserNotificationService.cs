using System;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IUserNotificationService
    {
        Task NotifyBetUpdateAsync(Guid playerId, object betDto);
        Task NotifyBetCancelledAsync(Guid playerId, object betDto);
        Task NotifyBetSettledAsync(Guid playerId, object betDto);
        Task NotifyWalletTransactionUpdateAsync(Guid playerId, object transaction);
    }
}

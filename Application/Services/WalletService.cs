using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IUserNotificationService _userNotificationService;

        public WalletService(IWalletRepository walletRepository, IWalletTransactionService walletTransactionService, IUserNotificationService userNotificationService)
        {
            _walletRepository = walletRepository;
            _walletTransactionService = walletTransactionService;
            _userNotificationService = userNotificationService;
        }

        public async Task<List<Wallet>> GetWalletsByPlayerIdAsync(Guid playerId)
        {
            return await _walletRepository.GetWalletsByPlayerIdAsync(playerId);
        }

        public async Task<Guid?> GetPlayerByWalletIdAsync(Guid walletId)
        {
            return await _walletRepository.GetPlayerByWalletIdAsync(walletId);
        }

        public async Task<Wallet?> GetWalletByIdAsync(Guid walletId)
        {
            return await _walletRepository.GetByIdAsync(walletId);
        }

        public async Task<bool> WalletExistsAsync(Guid walletId)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);
            return wallet != null;
        }

        public async Task UpdateWalletAsync(Wallet wallet)
        {
            await _walletRepository.UpdateAsync(wallet);
        }

        public async Task<WalletTransaction> DebitWalletAsync(Wallet wallet, decimal amount, Guid? betId = null)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                wallet.DebitWallet(amount);
                var transaction = await _walletTransactionService.DebitWalletAsync(wallet, amount, betId);

                // Notify user about wallet debit
                var playerId = await GetPlayerByWalletIdAsync(wallet.Id);
                if (playerId != null)
                {
                    var transactionDto = new WalletTransactionDTO
                    {
                        Id = transaction.Id,
                        WalletId = transaction.WalletId,
                        Amount = transaction.Amount,
                        TypeId = transaction.TransactionTypeId,
                        CreatedAt = transaction.CreatedAt,
                        BetId = transaction.BetId,
                    };
                    await _userNotificationService.NotifyWalletTransactionUpdateAsync(playerId.Value, transactionDto);
                }

                scope.Complete();
                return transaction;
            }
            catch
            {
                // Transaction will be rolled back if not completed
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }

        public async Task<WalletTransaction> CreditWalletAsync(Wallet wallet, decimal amount, Guid? betId = null)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                wallet.CreditWallet(amount);
                var transaction = await _walletTransactionService.CreditWalletAsync(wallet, amount, betId);

                // Notify user about wallet credit
                var playerId = await GetPlayerByWalletIdAsync(wallet.Id);
                if (playerId != null)
                {
                    var transactionDto = new WalletTransactionDTO
                    {
                        Id = transaction.Id,
                        WalletId = transaction.WalletId,
                        Amount = transaction.Amount,
                        TypeId = transaction.TransactionTypeId,
                        CreatedAt = transaction.CreatedAt,
                        BetId = transaction.BetId,
                    };
                    await _userNotificationService.NotifyWalletTransactionUpdateAsync(playerId.Value, transactionDto);
                }

                scope.Complete();
                return transaction;
            }
            catch
            {
                // Transaction will be rolled back if not completed
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }

        public async Task<WalletTransaction> CalculateWalletCheckpointAsync(Wallet wallet)
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                var transaction = await _walletTransactionService.CalculateWalletCheckpointAsync(wallet.Id);
                scope.Complete();
                return transaction;
            }
            catch
            {
                // Transaction will be rolled back if not completed
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }

        public async Task<Currency?> GetCurrencyByWalletIdAsync(Guid walletId)
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);
            if (wallet == null)
                return null;

            return wallet.Currency;
        }
    }
}

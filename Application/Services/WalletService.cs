using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletTransactionService _walletTransactionService;

        public WalletService(IWalletRepository walletRepository, IWalletTransactionService walletTransactionService)
        {
            _walletRepository = walletRepository;
            _walletTransactionService = walletTransactionService;
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
                var checkpoint = wallet.CalculateWalletCheckpoint();
                var transaction = await _walletTransactionService.CheckpointWalletAsync(wallet, checkpoint);
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
    }
}

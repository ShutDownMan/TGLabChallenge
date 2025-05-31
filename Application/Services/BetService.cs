using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

using TransactionTypeEnum = Domain.Enums.TransactionType;
using BetStatusEnum = Domain.Enums.BetStatus;

namespace Application.Services
{
    public class BetService : IBetService
    {
        private readonly IBetRepository _betRepository;
        private readonly IMapper _mapper;
        private readonly IWalletService _walletService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly ILogger<BetService> _logger;

        public BetService(
            IBetRepository betRepository,
            IMapper mapper,
            IWalletService walletService,
            IWalletTransactionService walletTransactionService,
            ILogger<BetService> logger)
        {
            _betRepository = betRepository;
            _mapper = mapper;
            _walletService = walletService;
            _walletTransactionService = walletTransactionService;
            _logger = logger;
        }

        public async Task<PlaceBetDTO> PlaceBetAsync(PlaceBetDTO betDTO)
        {
            _logger.LogInformation("Starting PlaceBetAsync for WalletId: {WalletId}, Amount: {Amount}", betDTO.WalletId, betDTO.Amount);

            var playerId = await _walletService.GetPlayerByWalletIdAsync(betDTO.WalletId);
            if (playerId == null)
            {
                _logger.LogWarning("Player not found for WalletId: {WalletId}", betDTO.WalletId);
                throw new InvalidOperationException("Player not found.");
            }

            var wallets = await _walletService.GetWalletsByPlayerIdAsync(playerId.Value);
            var wallet = wallets.FirstOrDefault(w => w.Id == betDTO.WalletId);

            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found. WalletId: {WalletId}", betDTO.WalletId);
                throw new InvalidOperationException("Wallet not found.");
            }

            if (wallet.Balance < betDTO.Amount)
            {
                _logger.LogWarning("Insufficient wallet balance. WalletId: {WalletId}, Balance: {Balance}, BetAmount: {Amount}", wallet.Id, wallet.Balance, betDTO.Amount);
                throw new InvalidOperationException("Insufficient wallet balance.");
            }

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                // Debit wallet using service (handles balance and transaction)
                await _walletService.DebitWalletAsync(wallet, betDTO.Amount, betDTO.Id);

                _logger.LogInformation("Debited wallet. WalletId: {WalletId}, NewBalance: {Balance}", wallet.Id, wallet.Balance);

                // Update wallet
                await _walletService.UpdateWalletAsync(wallet);
                _logger.LogInformation("Wallet updated. WalletId: {WalletId}", wallet.Id);

                var betEntity = _mapper.Map<Bet>(betDTO);
                betEntity.StatusId = (int)BetStatusEnum.Created;

                await _betRepository.AddAsync(betEntity);
                _logger.LogInformation("Bet added. BetId: {BetId}, WalletId: {WalletId}", betEntity.Id, wallet.Id);

                scope.Complete();

                _logger.LogInformation("PlaceBetAsync completed successfully for BetId: {BetId}", betEntity.Id);
                return _mapper.Map<PlaceBetDTO>(betEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in PlaceBetAsync for WalletId: {WalletId}", betDTO.WalletId);
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }

        public async Task CancelBetAsync(PlaceBetDTO betDto)
        {
            // TODO: Add any additional business logic for canceling a bet, such as validation or state checks

            var bet = _mapper.Map<Bet>(betDto);

            await _betRepository.CancelAsync(bet);
        }

        public async Task<PlaceBetDTO?> GetBetByIdAsync(Guid id)
        {
            var bet = await _betRepository.GetByIdAsync(id);

            return bet == null ? null : _mapper.Map<PlaceBetDTO>(bet);
        }

        public async Task<IEnumerable<PlaceBetDTO>> GetBetsByUserAsync(Guid userId)
        {
            var bets = await _betRepository.GetByUserAsync(userId);

            return _mapper.Map<IEnumerable<PlaceBetDTO>>(bets);
        }
    }
}

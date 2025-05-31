using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Services
{
    public class BetService : IBetService
    {
        private readonly IBetRepository _betRepository;
        private readonly IMapper _mapper;
        private readonly IWalletRepository _walletRepository;

        public BetService(IBetRepository betRepository, IMapper mapper, IWalletRepository walletRepository)
        {
            _betRepository = betRepository;
            _mapper = mapper;
            _walletRepository = walletRepository;
        }

        public async Task<BetDTO> PlaceBetAsync(BetDTO betDTO)
        {
            // Get playerId by wallet
            var playerId = await _walletRepository.GetPlayerByWalletIdAsync(betDTO.WalletId);
            if (playerId == null)
                throw new InvalidOperationException("Player not found.");

            var wallets = await _walletRepository.GetWalletsByPlayerIdAsync(playerId.Value);
            var wallet = wallets.FirstOrDefault(w => w.Id == betDTO.WalletId);

            if (wallet == null)
                throw new InvalidOperationException("Wallet not found.");

            if (wallet.Balance < betDTO.Amount)
                throw new InvalidOperationException("Insufficient wallet balance.");

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                // Decrement balance
                wallet.Balance -= betDTO.Amount;

                // Update wallet
                await _walletRepository.UpdateAsync(wallet);

                // Set default status id for new bet (Created = 1)
                var betEntity = _mapper.Map<Bet>(betDTO);
                betEntity.StatusId = 1;

                await _betRepository.AddAsync(betEntity);

                scope.Complete();

                return _mapper.Map<BetDTO>(betEntity);
            }
            catch
            {
                // Transaction will be rolled back if not completed
                throw;
            }
            finally
            {
                // Ensure the transaction scope is disposed
                scope.Dispose();
            }
        }

        public async Task CancelBetAsync(BetDTO betDto)
        {
            // TODO: Add any additional business logic for canceling a bet, such as validation or state checks

            var bet = _mapper.Map<Bet>(betDto);

            await _betRepository.CancelAsync(bet);
        }

        public async Task<BetDTO?> GetBetByIdAsync(Guid id)
        {
            var bet = await _betRepository.GetByIdAsync(id);

            return bet == null ? null : _mapper.Map<BetDTO>(bet);
        }

        public async Task<IEnumerable<BetDTO>> GetBetsByUserAsync(Guid userId)
        {
            var bets = await _betRepository.GetByUserAsync(userId);

            return _mapper.Map<IEnumerable<BetDTO>>(bets);
        }
    }
}

using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Models;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BetService : IBetService
    {
        private readonly IBetRepository _betRepository;
        private readonly IMapper _mapper;

        public BetService(IBetRepository betRepository, IMapper mapper)
        {
            _betRepository = betRepository;
            _mapper = mapper;
        }

        public async Task<BetDTO> PlaceBetAsync(BetDTO betDto)
        {
            // TODO: Add any additional business logic for placing a bet, such as validation or calculations

            var betEntity = _mapper.Map<Bet>(betDto);

            await _betRepository.AddAsync(betEntity);

            return _mapper.Map<BetDTO>(betEntity);
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

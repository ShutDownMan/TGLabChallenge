using Application.Interfaces.Services;
using Application.Models;
using Application.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BetController : ControllerBase
    {
        private readonly IBetService _betService;
        private readonly IValidator<PlaceBetDTO> _betDTOValidator;

        public BetController(IBetService betService, IValidator<PlaceBetDTO> betDTOValidator)
        {
            _betService = betService;
            _betDTOValidator = betDTOValidator;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceBet([FromBody] PlaceBetDTO placeBetDTO)
        {
            ValidationResult validationResult = await _betDTOValidator.ValidateAsync(placeBetDTO);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _betService.PlaceBetAsync(placeBetDTO);
            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBet(Guid id)
        {
            // TODO: Add any additional business logic for canceling a bet, such as validation or state checks
            // As well as Cancel Timestamp and other related fields if necessary
            await _betService.CancelBetAsync(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBet(Guid id)
        {
            var betDTO = await _betService.GetBetByIdAsync(id);
            if (betDTO == null)
                return NotFound();
            return Ok(betDTO);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBetsByUser(Guid userId)
        {
            var bets = await _betService.GetBetsByUserAsync(userId);
            return Ok(bets);
        }
    }
}

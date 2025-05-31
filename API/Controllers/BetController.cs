using Application.Services;
using Application.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BetController : ControllerBase
    {
        private readonly BetService _betService;
        private readonly IValidator<BetDto> _betDtoValidator;

        public BetController(BetService betService, IValidator<BetDto> betDtoValidator)
        {
            _betService = betService;
            _betDtoValidator = betDtoValidator;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceBet([FromBody] BetDto betDto)
        {
            ValidationResult validationResult = await _betDtoValidator.ValidateAsync(betDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _betService.PlaceBetAsync(betDto);
            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBet(Guid id)
        {
            var betDto = await _betService.GetBetByIdAsync(id);
            if (betDto == null)
                return NotFound();

            await _betService.CancelBetAsync(betDto);
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBet(Guid id)
        {
            var betDto = await _betService.GetBetByIdAsync(id);
            if (betDto == null)
                return NotFound();
            return Ok(betDto);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBetsByUser(Guid userId)
        {
            var bets = await _betService.GetBetsByUserAsync(userId);
            return Ok(bets);
        }
    }
}

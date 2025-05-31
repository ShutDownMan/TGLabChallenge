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
        private readonly IValidator<BetDTO> _betDTOValidator;

        public BetController(BetService betService, IValidator<BetDTO> betDTOValidator)
        {
            _betService = betService;
            _betDTOValidator = betDTOValidator;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceBet([FromBody] BetDTO betDTO)
        {
            ValidationResult validationResult = await _betDTOValidator.ValidateAsync(betDTO);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _betService.PlaceBetAsync(betDTO);
            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBet(Guid id)
        {
            var betDTO = await _betService.GetBetByIdAsync(id);
            if (betDTO == null)
                return NotFound();

            await _betService.CancelBetAsync(betDTO);
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

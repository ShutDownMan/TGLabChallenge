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
        private readonly IValidator<CancelBetDTO> _cancelBetDTOValidator;

        public BetController(IBetService betService, IValidator<PlaceBetDTO> betDTOValidator, IValidator<CancelBetDTO> cancelBetDTOValidator)
        {
            _betService = betService;
            _betDTOValidator = betDTOValidator;
            _cancelBetDTOValidator = cancelBetDTOValidator;
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
        public async Task<IActionResult> CancelBet(Guid id, [FromBody] CancelBetDTO cancelBetDTO)
        {
            var validationResult = await _cancelBetDTOValidator.ValidateAsync(cancelBetDTO);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                await _betService.CancelBetAsync(id, cancelBetDTO.CancelReason);
                return Ok(new { message = "Bet cancelled successfully.", betId = id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Bet not found." });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBet(Guid id)
        {
            var betDTO = await _betService.GetBetByIdAsync(id);
            if (betDTO == null)
                return NotFound();
            return Ok(betDTO);
        }

        [HttpPost("{id}/settle")]
        public async Task<IActionResult> SettleBet(Guid id)
        {
            try
            {
                var result = await _betService.SettleBetAsync(id);
                return Ok(new { message = "Bet settled successfully.", betId = id, result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Bet not found." });
            }
        }
    }
}

using Application.Interfaces.Services;
using Application.Models;
using Application.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
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
        private readonly IWalletService _walletService;

        public BetController(IBetService betService, IValidator<PlaceBetDTO> betDTOValidator, IValidator<CancelBetDTO> cancelBetDTOValidator, IWalletService walletService)
        {
            _betService = betService;
            _betDTOValidator = betDTOValidator;
            _cancelBetDTOValidator = cancelBetDTOValidator;
            _walletService = walletService;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Place a bet",
            Description = "Places a new bet with the provided details.\n" +
                          "The bet amount must meet the game's minimum requirements,\n" +
                          "and the wallet must have sufficient balance.",
            OperationId = "PlaceBet"
        )]
        [SwaggerResponse(200, "Bet placed successfully", typeof(BetDTO))]
        [SwaggerResponse(400, "Validation failed or insufficient wallet balance")]
        public async Task<IActionResult> PlaceBet([FromBody] PlaceBetDTO placeBetDTO)
        {
            // <see cref="Application.Models.PlaceBetDTOValidator"/>
            ValidationResult validationResult = await _betDTOValidator.ValidateAsync(placeBetDTO);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var result = await _betService.PlaceBetAsync(placeBetDTO);
            return Ok(result);
        }

        [HttpPost("{id}/cancel")]
        [SwaggerOperation(
            Summary = "Cancel a bet",
            Description = "Cancels an existing bet with the provided reason.\n" +
                          "A cancellation tax may apply, reducing the refund amount.",
            OperationId = "CancelBet"
        )]
        [SwaggerResponse(200, "Bet cancelled successfully", typeof(object))]
        [SwaggerResponse(400, "Validation failed or invalid operation")]
        [SwaggerResponse(404, "Bet not found")]
        public async Task<IActionResult> CancelBet(Guid id, [FromBody] CancelBetDTO cancelBetDTO)
        {
            // <see cref="Application.Models.CancelBetDTOValidator"/>
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
        [SwaggerOperation(
            Summary = "Get bet details",
            Description = "Retrieves details of a specific bet by its ID.\n" +
                          "Returns null if the bet is not found.",
            OperationId = "GetBet"
        )]
        [SwaggerResponse(200, "Bet details retrieved successfully", typeof(BetDTO))]
        [SwaggerResponse(404, "Bet not found")]
        public async Task<IActionResult> GetBet(Guid id)
        {
            var playerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerIdClaim) || !Guid.TryParse(playerIdClaim, out var playerId))
                return Unauthorized();

            var betDTO = await _betService.GetBetByIdAsync(id);
            if (betDTO == null)
                return NotFound();

            var walletPlayerId = await _walletService.GetPlayerByWalletIdAsync(betDTO.WalletId);
            if (walletPlayerId == null || walletPlayerId != playerId)
                return Unauthorized(new { message = "Quit snooping! This bet is not yours." });

            return Ok(betDTO);
        }

        [HttpPost("{id}/settle")]
        [SwaggerOperation(
            Summary = "Settle a bet",
            Description = "Settles a specific bet by its ID.\n" +
                          "Determines whether the player wins or loses based on random logic.\n" +
                          "If the player wins, the payout is credited to their wallet.",
            OperationId = "SettleBet"
        )]
        [SwaggerResponse(200, "Bet settled successfully", typeof(BetDTO))]
        [SwaggerResponse(400, "Invalid operation")]
        [SwaggerResponse(404, "Bet not found")]
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

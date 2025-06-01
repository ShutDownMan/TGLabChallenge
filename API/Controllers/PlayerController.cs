using Application.Interfaces.Services;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlayerController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayerController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpGet("profile")]
        [SwaggerOperation(
            Summary = "Get player profile",
            Description = "Retrieves the profile of the authenticated player."
        )]
        [SwaggerResponse(200, "Player profile retrieved successfully", typeof(PlayerProfileDTO))]
        [SwaggerResponse(401, "Unauthorized")]
        [SwaggerResponse(404, "Player profile not found")]
        public async Task<IActionResult> GetProfile()
        {
            // Extract player id from JWT claims
            var playerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerIdClaim) || !Guid.TryParse(playerIdClaim, out var playerId))
                return Unauthorized();

            var profile = await _playerService.GetProfileAsync(playerId);
            if (profile == null)
                return NotFound();

            return Ok(profile);
        }

        [HttpGet("bets")]
        [SwaggerOperation(
            Summary = "Get player bets",
            Description = "Retrieves all bets placed by the authenticated player."
        )]
        [SwaggerResponse(200, "Player bets retrieved successfully", typeof(IEnumerable<BetDTO>))]
        [SwaggerResponse(401, "Unauthorized")]
        public async Task<IActionResult> GetBets()
        {
            var playerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerIdClaim) || !Guid.TryParse(playerIdClaim, out var playerId))
                return Unauthorized();

            var bets = await _playerService.GetBetsAsync(playerId);
            return Ok(bets);
        }

        [HttpGet("wallet-transactions")]
        [SwaggerOperation(
            Summary = "Get wallet transactions",
            Description = "Retrieves all wallet transactions of the authenticated player."
        )]
        [SwaggerResponse(200, "Wallet transactions retrieved successfully", typeof(IEnumerable<WalletTransactionDTO>))]
        [SwaggerResponse(401, "Unauthorized")]
        public async Task<IActionResult> GetWalletTransactions()
        {
            var playerIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(playerIdClaim) || !Guid.TryParse(playerIdClaim, out var playerId))
                return Unauthorized();

            var transactions = await _playerService.GetWalletTransactionsAsync(playerId);
            return Ok(transactions);
        }
    }
}

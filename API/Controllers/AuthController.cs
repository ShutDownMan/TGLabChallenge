using Application.Interfaces.Services;
using Application.Models;
using Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FluentValidation.Results;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterRequest> _registerValidator;
        private readonly IValidator<TokenRequest> _tokenValidator;
        private readonly IValidator<LoginRequest> _loginValidator;

        public AuthController(
            IAuthService authService,
            IValidator<RegisterRequest> registerValidator,
            IValidator<TokenRequest> tokenValidator,
            IValidator<LoginRequest> loginValidator)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _tokenValidator = tokenValidator;
            _loginValidator = loginValidator;
        }

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Authenticate user and return a token",
            Description = "Validates user credentials (username or email) and generates a JWT token.\n" +
                  "Throws UnauthorizedAccessException if credentials are invalid."
        )]
        [SwaggerResponse(200, "Authentication successful", typeof(object))]
        [SwaggerResponse(400, "Validation failed")]
        [SwaggerResponse(401, "Unauthorized access")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // <see cref="Application.Models.LoginRequestValidator" />
            ValidationResult validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult);
            }

            try
            {
                var token = await _authService.LoginAsync(request.Identifier, request.Password);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Register a new user",
            Description = "Creates a new user account with the provided details.\n" +
                  "Throws UserAlreadyExistsException if username or email is already registered.\n" +
                  "Throws InvalidOperationException if the provided currency ID is invalid."
        )]
        [SwaggerResponse(200, "Registration successful")]
        [SwaggerResponse(400, "Validation failed or user already exists")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // <see cref="Application.Models.RegisterRequestValidator" />
            ValidationResult validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                await _authService.RegisterAsync(
                    request.Username,
                    request.Password,
                    request.Email,
                    request.CurrencyId,
                    request.InitialBalance
                );
                return Ok();
            }
            catch (UserAlreadyExistsException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("validate-token")]
        [SwaggerOperation(
            Summary = "Validate a token",
            Description = "Checks if the provided JWT token is valid. Returns true if valid, otherwise false."
        )]
        [SwaggerResponse(200, "Token validation result", typeof(object))]
        [SwaggerResponse(400, "Validation failed")]
        public async Task<IActionResult> ValidateToken([FromBody] TokenRequest request)
        {
            // <see cref="Application.Models.TokenRequestValidator" />
            ValidationResult validationResult = await _tokenValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var isValid = await _authService.ValidateTokenAsync(request.Token);
            return Ok(new { isValid });
        }
    }
}

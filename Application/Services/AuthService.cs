using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces;
using Application.Exceptions;
using Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    /// <summary>
    /// Service responsible for handling authentication-related operations.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IPlayerService _playerService;
        private readonly ICurrencyService _currencyService;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IWalletService _walletService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IPlayerService playerService,
            ICurrencyService currencyService,
            IJwtTokenGenerator jwtTokenGenerator,
            IWalletService walletService,
            IWalletTransactionService walletTransactionService,
            ILogger<AuthService> logger)
        {
            _playerService = playerService;
            _currencyService = currencyService;
            _jwtTokenGenerator = jwtTokenGenerator;
            _walletService = walletService;
            _walletTransactionService = walletTransactionService;
            _logger = logger;
        }

        /// <summary>
        /// Logs in a user using their identifier (username or email) and password.
        /// </summary>
        /// <param name="identifier">The username or email of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>A JWT token if authentication is successful.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if the credentials are invalid.</exception>
        public async Task<string> LoginAsync(string identifier, string password)
        {
            _logger.LogDebug("Starting LoginAsync for Identifier: {Identifier}", identifier);

            Player? user = await _playerService.GetByUsernameAsync(identifier);
            if (user == null)
            {
                _logger.LogDebug("User not found by username. Trying email for Identifier: {Identifier}", identifier);
                user = await _playerService.GetByEmailAsync(identifier);
            }

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid credentials for Identifier: {Identifier}", identifier);
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            _logger.LogInformation("Login successful for UserId: {UserId}", user.Id);
            return _jwtTokenGenerator.GenerateToken(user);
        }

        /// <summary>
        /// Registers a new user with the provided details.
        /// </summary>
        /// <param name="username">The username of the new user.</param>
        /// <param name="password">The password of the new user.</param>
        /// <param name="email">The email of the new user.</param>
        /// <param name="currencyId">The ID of the currency for the user's wallet. Defaults to the system's default currency if null.</param>
        /// <param name="initialBalance">The initial balance for the user's wallet. Defaults to 0 if null.</param>
        /// <exception cref="UserAlreadyExistsException">Thrown if the username or email is already registered.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the provided currency ID is invalid.</exception>
        public async Task RegisterAsync(string username, string password, string email, int? currencyId, decimal? initialBalance)
        {
            _logger.LogDebug("Starting RegisterAsync for Username: {Username}, Email: {Email}", username, email);

            // Check if username is already taken
            if (await _playerService.UsernameExistsAsync(username))
            {
                _logger.LogWarning("Username already exists: {Username}", username);
                throw new UserAlreadyExistsException("Username already exists.");
            }

            // check if email is already registered
            if (await _playerService.EmailExistsAsync(email))
            {
                _logger.LogWarning("Email already registered: {Email}", email);
                throw new UserAlreadyExistsException("Email already registered.");
            }

            // Validate currency ID
            if (currencyId == null)
            {
                _logger.LogDebug("CurrencyId not provided. Fetching default currency.");
                var defaultCurrency = await _currencyService.GetDefaultCurrencyAsync();
                currencyId = defaultCurrency.Id;
            }
            else if (!await _currencyService.CurrencyExistsAsync(currencyId.Value))
            {
                _logger.LogWarning("Invalid currency ID: {CurrencyId}", currencyId);
                throw new InvalidOperationException("Invalid currency ID.");
            }

            // Hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            // Use a transaction scope to ensure atomicity
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                // Create the user
                var player = new Player
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    PasswordHash = hashedPassword,
                    Email = email,
                    CreatedAt = DateTime.UtcNow
                };
                await _playerService.AddAsync(player);
                _logger.LogDebug("Player created with ID: {PlayerId}", player.Id);

                // Create wallet for the user with initial balance
                var wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    PlayerId = player.Id,
                    CurrencyId = currencyId.Value,
                    Balance = initialBalance ?? 0m,
                    CreatedAt = DateTime.UtcNow
                };
                await _walletService.CreateWalletAsync(wallet);
                _logger.LogDebug("Wallet created with ID: {WalletId} for PlayerId: {PlayerId}", wallet.Id, player.Id);

                // Create initial checkpoint transaction for the wallet
                await _walletTransactionService.CheckpointWalletAsync(wallet, wallet.Balance, null);
                _logger.LogDebug("Initial checkpoint transaction created for WalletId: {WalletId}", wallet.Id);

                // Commit the transaction
                scope.Complete();
                _logger.LogInformation("RegisterAsync completed successfully for Username: {Username}, Email: {Email}", username, email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during RegisterAsync for Username: {Username}, Email: {Email}", username, email);
                throw;
            }
            finally
            {
                // Ensure the transaction scope is disposed
                scope.Dispose();
            }
        }

        /// <summary>
        /// Validates a given JWT token.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>True if the token is valid; otherwise, false.</returns>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            _logger.LogDebug("Starting ValidateTokenAsync for Token: {Token}", token);
            var isValid = _jwtTokenGenerator.ValidateToken(token);
            _logger.LogDebug("Token validation result for Token: {Token} is {IsValid}", token, isValid);
            return isValid;
        }
    }
}

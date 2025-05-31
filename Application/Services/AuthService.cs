using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Interfaces;
using Application.Exceptions;
using Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPlayerRepository _userRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IWalletRepository _walletRepository;
        private readonly IWalletTransactionService _walletTransactionService;

        // FIXME: use services instead of repositories directly
        public AuthService(
            IPlayerRepository userRepository,
            ICurrencyRepository currencyRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IWalletRepository walletRepository,
            IWalletTransactionService walletTransactionService)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _currencyRepository = currencyRepository;
            _walletRepository = walletRepository;
            _walletTransactionService = walletTransactionService;
        }

        public async Task<string> LoginAsync(string identifier, string password)
        {
            Player? user = await _userRepository.GetByUsernameAsync(identifier);
            if (user == null)
            {
                // Try by email if not found by username
                user = await _userRepository.GetByEmailAsync(identifier);
            }
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            return _jwtTokenGenerator.GenerateToken(user);
        }

        public async Task RegisterAsync(string username, string password, string email, int? currencyId, decimal? initialBalance)
        {
            // Check if username is already taken
            if (await _userRepository.UsernameExistsAsync(username))
            {
                throw new UserAlreadyExistsException("Username already exists.");
            }

            // check if email is already registered
            if (await _userRepository.EmailExistsAsync(email))
            {
                throw new UserAlreadyExistsException("Email already registered.");
            }

            // Validate currency ID
            if (currencyId == null)
            {
                var defaultCurrency = await _currencyRepository.GetDefaultCurrencyAsync();
                currencyId = defaultCurrency.Id;
            }
            else if (!await _currencyRepository.CurrencyExistsAsync(currencyId.Value))
            {
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
                await _userRepository.AddAsync(player);

                // Create wallet for the user with initial balance
                var wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    PlayerId = player.Id,
                    CurrencyId = currencyId.Value,
                    Balance = initialBalance ?? 0m,
                    CreatedAt = DateTime.UtcNow
                };
                await _walletRepository.AddAsync(wallet);

                // Create initial checkpoint transaction for the wallet
                await _walletTransactionService.CheckpointWalletAsync(wallet, wallet.Balance);

                // Commit the transaction
                scope.Complete();
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

        public async Task<bool> ValidateTokenAsync(string token)
        {
            return _jwtTokenGenerator.ValidateToken(token);
        }
    }
}

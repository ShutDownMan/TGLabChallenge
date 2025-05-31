using Application.Interfaces;
using Application.Exceptions;
using Domain.Entities;
using Domain.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IPlayerRepository _userRepository;
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IPlayerRepository userRepository, ICurrencyRepository currencyRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _currencyRepository = currencyRepository;
        }

        public async Task<string> LoginAsync(string usernameOrEmail, string password)
        {
            Player? user = await _userRepository.GetByUsernameAsync(usernameOrEmail);
            if (user == null)
            {
                // Try by email if not found by username
                user = await _userRepository.GetByEmailAsync(usernameOrEmail);
            }
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            return _jwtTokenGenerator.GenerateToken(user);
        }

        public async Task RegisterAsync(string username, string password, string email, int? currencyId)
        {
            if (await _userRepository.UsernameExistsAsync(username))
            {
                throw new UserAlreadyExistsException("Username already exists.");
            }

            if (currencyId == null)
            {
                var defaultCurrency = await _currencyRepository.GetDefaultCurrencyAsync();
                currencyId = defaultCurrency.Id;
            }
            else if (!await _currencyRepository.CurrencyExistsAsync(currencyId.Value))
            {
                throw new InvalidOperationException("Invalid currency ID.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new Player
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = hashedPassword,
                Email = email,
                CurrencyId = currencyId.Value,
                Balance = 0.00m,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            return _jwtTokenGenerator.ValidateToken(token);
        }
    }
}

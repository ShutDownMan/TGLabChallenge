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
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            return _jwtTokenGenerator.GenerateToken(user);
        }

        public async Task RegisterAsync(string username, string password)
        {
            if (await _userRepository.UsernameExistsAsync(username))
            {
                throw new UserAlreadyExistsException("Username already exists.");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new Player
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = hashedPassword
            };

            await _userRepository.AddAsync(user);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            return _jwtTokenGenerator.ValidateToken(token);
        }
    }
}

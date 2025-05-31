using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(string username, string password);
        Task RegisterAsync(string username, string password, string email, int? currencyId, decimal? initialBalance);
        Task<bool> ValidateTokenAsync(string token);
    }
}
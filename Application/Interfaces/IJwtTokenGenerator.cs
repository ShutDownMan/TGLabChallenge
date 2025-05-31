using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Security
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(Player user);
        bool ValidateToken(string token);
    }

}

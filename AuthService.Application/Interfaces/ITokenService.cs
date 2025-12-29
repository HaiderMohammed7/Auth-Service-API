using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, List<string> roles);
    }
}
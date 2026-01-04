using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        void Add(RefreshToken token);
        void Update(RefreshToken token);
        RefreshToken? GetByToken(string token);
        List<RefreshToken> GetActiveTokensByUser(int userId);
    }
}
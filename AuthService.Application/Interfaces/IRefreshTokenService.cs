using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces
{
    public interface IRefreshTokenService
    {
        RefreshToken Create(int userID, string ipAddress);
        RefreshToken? Get(string token);
        void Revoke(string token, string ipAddress);
        void RevokeAllForUser(int userId, string ipAddress);

    }
}
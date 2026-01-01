using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

namespace AuthService.Application.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public RefreshToken Create(int userId, string ipAddress)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var refreshToken = new RefreshToken
            {
                UserID = userId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(14),
                CreatedByIP = ipAddress
            };

            _refreshTokenRepository.Add(refreshToken);
            return refreshToken;
        }

        public RefreshToken? Get(string token) => _refreshTokenRepository.GetByToken(token);

        public void Revoke(string token, string ipAddress)
        {
            var refreshToken = _refreshTokenRepository.GetByToken(token);
            if (refreshToken == null) return;

            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByIP = ipAddress;

            _refreshTokenRepository.Update(refreshToken);
        }
    }
}
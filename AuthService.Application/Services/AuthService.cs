using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Shared.DTOs;

namespace AuthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public AuthService(IUserRepository userRepo, IPasswordHasher passwordHasher, ITokenService tokenService, IRefreshTokenService refreshTokenService)
        {
            _userRepo = userRepo;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
        }

        public TokenResponseDto Login(LoginRequestDto request, string ipAddress)
        {
            var user = _userRepo.GetByEmail(request.Email);

            if(user == null)
                throw new AppException("Invalid credentials", 401);

            if(!user.IsActive)
                throw new AppException("Account disabled", 403);

            if (user.IsLocked)
                throw new AppException("Account is Locked. Try Later.", 423);

            var isValidPassword = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);

            if(!isValidPassword)
            {
                _userRepo.IncrementFailedAttempts(user.UserID);

                if(user.FailedLoginAttempts + 1 >= 5)
                    _userRepo.LockUser(user.UserID);

                throw new AppException("Invalid credentials", 401);
            }

            _userRepo.ResetFailedAttempts(user.UserID);
            _userRepo.UpdateLastLogin(user.UserID);

            var roles = _userRepo.GetUserRoles(user.UserID) ?? new List<string>();

            var accessToken = _tokenService.GenerateAccessToken(user, roles);

            var refreshToken = _refreshTokenService.Create(user.UserID, ipAddress);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            };
        }

        public TokenResponseDto Refresh(string refreshToken, string ipAddress)
        {
            var existingToken = _refreshTokenService.Get(refreshToken);

            if (existingToken == null || existingToken.RevokedAt != null || existingToken.ExpiresAt < DateTime.UtcNow)
                throw new Exception("Invalid refresh token");

            var user = _userRepo.GetById(existingToken.UserID) ?? throw new Exception("User not found");

            var roles = _userRepo.GetUserRoles(user.UserID);

            _refreshTokenService.Revoke(refreshToken, ipAddress);
            var newRefreshToken = _refreshTokenService.Create(user.UserID, ipAddress);

            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);

            return new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            };
        }

        public void Logout(string refreshToken, string ipAddress)
        {
            _refreshTokenService.Revoke(refreshToken, ipAddress);
        }
        public void LogoutAll(int userId, string ipAddress)
        {
            _refreshTokenService.RevokeAllForUser(userId, ipAddress);
        }
    }
}
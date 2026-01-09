using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
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
            var user = _userRepo.GetByLoginIdentifier(request.Identifier);

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

        public void Register(RegisterRequestDto dto)
        {
            var existingUser = _userRepo.GetByEmailOrUserName(dto.Email, dto.UserName);

            if (existingUser != null)
            {
                if (existingUser.Email == dto.Email)
                    throw new AppException("Email already exists", 409);

                if (existingUser.UserName == dto.UserName)
                    throw new AppException("UserName already exists", 409);
            }


            _passwordHasher.CreatePasswordHash(dto.Password, out var hash, out var salt);

            var user = new User
            {
                Email = dto.Email,
                UserName = dto.UserName,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsActive = true,
                IsLocked = false,
                FailedLoginAttempts = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _userRepo.Add(user);
            _userRepo.AssignRole(user.UserID, "User");
        }

        public void ChangePassword(int userId, ChangePasswordRequestDto dto, string ipAddress)
        {
            var user = _userRepo.GetById(userId)
                ?? throw new AppException("User not found", 404);

            var isValid = _passwordHasher.VerifyPassword
                (dto.CurrentPassword,
                user.PasswordHash,
                user.PasswordSalt);

            if (!isValid) throw new AppException("Current password is incorrect", 400);

            _passwordHasher.CreatePasswordHash
                (dto.NewPassword,
                out var newHash,
                out var newSalt);

            _userRepo.UpdatePassword(userId, newHash, newSalt);

            _refreshTokenService.RevokeAllForUser(userId, ipAddress);
        }

        public void ForgotPassword(ForgotPasswordRequestDto dto)
        {
            var user = _userRepo.GetByEmail(dto.Email);
            if (user == null) return;

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            using var sha = SHA256.Create();
            var tokenHash = sha.ComputeHash(Encoding.UTF8.GetBytes(rawToken));

            var resetToken = new PasswordResetToken
            {
                UserID = user.UserID,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                CreatedAt = DateTime.UtcNow
            };

            _userRepo.AddResetToken(resetToken);

            //هذا لاحقًا: Send Email (rawToken)
        }

        public void ResetPassword(ResetPasswordRequestDto dto, string ipAddress)
        {
            using var sha = SHA256.Create();
            var tokenHash = sha.ComputeHash(Encoding.UTF8.GetBytes(dto.Token));

            var resetToken = _userRepo.GetValidResetToken(tokenHash)
                ?? throw new AppException("Invalid or expired token", 400);

            var user = _userRepo.GetById(resetToken.UserID)
                ?? throw new AppException("User not found", 400);

            _passwordHasher.CreatePasswordHash(dto.NewPassword, out var hash, out var salt);

            _userRepo.UpdatePassword(user.UserID, hash, salt);
            _userRepo.MarkResetTokenUsed(resetToken.ID);

            _refreshTokenService.RevokeAllForUser(user.UserID, ipAddress);
        }
    }
}
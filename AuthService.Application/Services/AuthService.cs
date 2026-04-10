using AuthService.Application.Exceptions;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Shared.DTOs;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Cryptography;
using System.Text;


namespace AuthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<AuthService> _logger;
        private readonly IAuditService _auditService;

        public AuthService(IUserRepository userRepo, IPasswordHasher passwordHasher,
            ITokenService tokenService, IRefreshTokenService refreshTokenService,
            ILogger<AuthService> logger, IAuditService auditService)
        {
            _userRepo = userRepo;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
            _auditService = auditService;
        }

        public TokenResponseDto Login(LoginRequestDto request, string ipAddress)
        {
            var user = _userRepo.GetByLoginIdentifier(request.Identifier);

            if (user == null)
            {
                _logger.LogWarning("Login failed. User not found. Identifier: {Identifier}, IP: {IP}", request.Identifier, ipAddress);

                _auditService.Log(null, "LoginFailed", request.Identifier, ipAddress);

                throw new AppException("Invalid credentials", 401);
            }


            if (!user.IsActive)
            {
                _logger.LogWarning("Login attempt for disabled account. UserId: {UserId}, IP: {IP}", user.UserID, ipAddress);

                throw new AppException("Account disabled", 403);
            }


            if (user.IsLocked)
            {
                _logger.LogWarning("Login attempt for locked account. UserId: {UserId}, IP: {IP}", user.UserID, ipAddress);

                throw new AppException("Account is Locked. Try Later.", 423);
            }


            var isValidPassword = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);

            if (!isValidPassword)
            {
                _logger.LogWarning("Invalid password. UserId: {UserId}, IP: {IP}", user.UserID, ipAddress);

                _auditService.Log(null, "LoginFailed", request.Identifier, ipAddress);

                _userRepo.IncrementFailedAttempts(user.UserID);

                if (user.FailedLoginAttempts + 1 >= 5)
                {
                    _logger.LogWarning("User locked after failed attempts. UserId: {UserId}, IP: {IP}", user.UserID, ipAddress);

                    _userRepo.LockUser(user.UserID);
                }

                throw new AppException("Invalid credentials", 401);
            }

            _logger.LogInformation("User logged in successfully. UserId: {UserId}, IP: {IP}", user.UserID, ipAddress);
            _auditService.Log(user.UserID, "Login", null, ipAddress);

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
            {
                _logger.LogWarning("Invalid refresh token attempt from IP: {IP}", ipAddress);

                throw new Exception("Invalid refresh token");
            }
                

            var user = _userRepo.GetById(existingToken.UserID) ?? throw new Exception("User not found");

            var roles = _userRepo.GetUserRoles(user.UserID);

            _refreshTokenService.Revoke(refreshToken, ipAddress);
            var newRefreshToken = _refreshTokenService.Create(user.UserID, ipAddress);

            var newAccessToken = _tokenService.GenerateAccessToken(user, roles);

            _logger.LogInformation("Token refreshed successfully. UserId: {UserId}, IP: {IP}", user.UserID,ipAddress);

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
            _auditService.Log(userId, "Logout", null, ipAddress);
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

            if (!isValid)
            {
                _logger.LogWarning("Change password failed. Incorrect current password. UserId: {UserId}, IP: {IP}", userId, ipAddress);

                throw new AppException("Current password is incorrect", 400);
            }

            _passwordHasher.CreatePasswordHash(dto.NewPassword,out var newHash,out var newSalt);

            _userRepo.UpdatePassword(userId, newHash, newSalt);

            _logger.LogInformation("Password changed successfully. UserId: {UserId}, IP: {IP}",userId, ipAddress);

            _refreshTokenService.RevokeAllForUser(userId, ipAddress);

            _auditService.Log(userId, "ChangePassword", null, ipAddress);
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

            Console.WriteLine("================================");
            Console.WriteLine($"Password reset token for {user.Email}");
            Console.WriteLine($"Token: {rawToken}");
            Console.WriteLine("================================");
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

            _auditService.Log(user.UserID, "ResetPassword", null, ipAddress);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthService.Application.Interfaces;
using AuthService.Shared.DTOs;

namespace AuthService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository userRepo, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _userRepo = userRepo;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public TokenResponseDto Login(LoginRequestDto request, string ipAddress)
        {
            var user = _userRepo.GetByEmail(request.Email);

            if(user == null || !user.IsActive || user.IsLocked)
                throw new Exception("Invalid credentials");

            var isValidPassword = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt);

            if(!isValidPassword)
            {
                _userRepo.IncrementFailedAttempts(user.UserID);

                if(user.FailedLoginAttempts + 1 >= 5)
                    _userRepo.LockUser(user.UserID);

                throw new Exception("Invalid credentials");
            }

            _userRepo.ResetFailedAttempts(user.UserID);
            _userRepo.UpdateLastLogin(user.UserID);

            var roles = _userRepo.GetUserRoles(user.UserID);

            var accessToken = _tokenService.GenerateAccessToken(user, roles);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = "WILL_BE_IMPLEMENTED_TOMORROW",
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            };
        }
    }
}
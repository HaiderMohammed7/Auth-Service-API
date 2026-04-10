using System.Security.Claims;
using AuthService.Application.Interfaces;
using AuthService.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using AuthService.Shared.Responses;
using Microsoft.AspNetCore.RateLimiting;

namespace AuthService.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = _authService.Refresh(dto.RefreshToken, ip);
            return Ok(ApiResponse<TokenResponseDto>.Ok(result));
        }

        [AllowAnonymous]
        [EnableRateLimiting("auth")]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = _authService.Login(dto,ip);
            return Ok(ApiResponse<TokenResponseDto>.Ok(result));
        }
        
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var email = User.FindFirstValue(ClaimTypes.Email);

            var userName = User.FindFirstValue(ClaimTypes.Name);

            var roles = User.FindAll(ClaimTypes.Role)
                .Select(r => r.Value)
                .ToList();


            return Ok(new UserDto
            {
                UserID = int.Parse(userId!),
                Email = email,
                UserName = userName,
                Roles = roles
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] RefreshTokenRequestDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _authService.Logout(dto.RefreshToken, ip);
            return NoContent();
        }

        [HttpPost("logout-all")]
        public IActionResult LogoutAll()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            _authService.LogoutAll(userId, ip);
            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Rigister([FromBody] RegisterRequestDto dto)
        {
            _authService.Register(dto);
            return Ok(ApiResponse<string>.Ok(null, "User registered successfully"));
        }

        [HttpPost("change-password")]
        public IActionResult ChangePassword(ChangePasswordRequestDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            _authService.ChangePassword(userId, dto, ip);
            return Ok(ApiResponse<string>.Ok(null, "Password changed successfully"));
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordRequestDto dto)
        {
            _authService.ForgotPassword(dto);
            return Ok(ApiResponse<string>.Ok(null, "If the email exists, a reset Link was sent"));
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public IActionResult ResetPassword(ResetPasswordRequestDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _authService.ResetPassword(dto, ip);
            return Ok(ApiResponse<string>.Ok(null, "Password reset successflly"));
        }
    }
}
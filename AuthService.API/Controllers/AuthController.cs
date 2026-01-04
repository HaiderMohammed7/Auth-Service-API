using System.Security.Claims;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using AuthService.Shared.Responses;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshTokenRequestDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = _authService.Refresh(dto.RefreshToken, ip);
            return Ok(result);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = _authService.Login(dto,ip);
            return Ok(ApiResponse<TokenResponseDto>.Ok(result));
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var email = User.FindFirstValue(ClaimTypes.Email);
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            return Ok(new UserDto
            {
                UserID = int.Parse(userId!),
                Email = email,
                Roles = roles
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout([FromBody] RefreshTokenRequestDto dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _authService.Logout(dto.RefreshToken, ip);
            return NoContent();
        }

        [Authorize]
        [HttpPost("logout-all")]
        public IActionResult LogoutAll()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

            _authService.LogoutAll(userId, ip);
            return NoContent();
        }
    }
}
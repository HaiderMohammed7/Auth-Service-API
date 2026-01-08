using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuthService.Shared.DTOs;

namespace AuthService.Application.Interfaces
{
    public interface IAuthService
    {
        TokenResponseDto Login(LoginRequestDto request, string ipAddress);
        TokenResponseDto Refresh(string refreshToken, string ipAddress);
        void Logout(string refreshToken, string ipAddress);
        void LogoutAll(int userId, string ipAddress);
        void Register(RegisterRequestDto dto);
    }
}
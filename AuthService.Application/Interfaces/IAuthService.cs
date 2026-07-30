using AuthService.Shared.DTOs;

namespace AuthService.Application.Interfaces
{
    public interface IAuthService
    {
        TokenResponseDto Login(LoginRequestDto request, string ipAddress);
        TokenResponseDto Refresh(string refreshToken, string ipAddress);
        void Logout(string refreshToken, string ipAddress);
        void LogoutAll(int userId, string ipAddress);
        int Register(RegisterRequestDto dto);
        void ChangePassword(int userId, ChangePasswordRequestDto dto, string ipAddress);
        void ForgotPassword(ForgotPasswordRequestDto dto);
        void ResetPassword(ResetPasswordRequestDto dto, string ipAddress);
        Task<List<UserBasicInfoDto>> GetUsersBasicInfoAsync(IEnumerable<int> userIds);
        Task<UserBasicInfoDto?> GetUserBasicInfoAsync(int userId);
        void UpdateUser(int authUserId, UpdateUserRequestDto dto);
        void DeleteUser(int userId, string ipAddress);
    }
}
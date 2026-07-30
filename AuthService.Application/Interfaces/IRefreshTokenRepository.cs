using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        void Add(RefreshToken token);
        void Save();
        RefreshToken? GetByToken(string token);
        List<RefreshToken> GetActiveTokensByUser(int userId);
        void DeleteAllForUser(int userId);
    }
}
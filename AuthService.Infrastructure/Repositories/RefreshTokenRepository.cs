using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;

namespace AuthService.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AuthDbContext _context;

        public RefreshTokenRepository(AuthDbContext context)
        {
            _context = context;
        }

        public void Add(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            _context.SaveChanges();
        }

        public RefreshToken? GetByToken(string token)
            => _context.RefreshTokens.SingleOrDefault(t => t.Token == token);

        public void Save ()
        {
            _context.SaveChanges();
        }

        public List<RefreshToken> GetActiveTokensByUser(int userId)
            => _context.RefreshTokens.Where(t => t.UserID == userId
            && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow).ToList();

        public void DeleteAllForUser(int userId)
        {
            var tokens = _context.RefreshTokens.Where(rt => rt.UserID == userId).ToList();

            if (tokens.Any())
            {
                _context.RefreshTokens.RemoveRange(tokens);
                _context.SaveChanges();
            }
        }
    }
}
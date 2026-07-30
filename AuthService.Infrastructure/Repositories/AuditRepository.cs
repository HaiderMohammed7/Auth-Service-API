using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;

namespace AuthService.Infrastructure.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly AuthDbContext _context;

        public AuditRepository(AuthDbContext context)
        {
            _context = context;
        }

        public void Add(AuditLog log)
        {
            _context.AuditLogs.Add(log);
            _context.SaveChanges();
        }
        public void DeleteAllForUser(int userId)
        {
            var logs = _context.AuditLogs.Where(a => a.UserID == userId).ToList();

            if (logs.Any())
            {
                _context.AuditLogs.RemoveRange(logs);
                _context.SaveChanges();
            }
        }
    }
}
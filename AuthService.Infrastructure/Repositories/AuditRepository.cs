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
    }
}
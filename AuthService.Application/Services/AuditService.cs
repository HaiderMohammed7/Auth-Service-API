using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

namespace AuthService.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _repo;

        public AuditService(IAuditRepository repo)
        {
            _repo = repo;
        }

        public void Log(int? userId, string action, string? description, string ipAddress)
        {
            var log = new AuditLog
            {
                UserID = userId,
                Action = action,
                Description = description,
                IPAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            _repo.Add(log);
        }
    }
}
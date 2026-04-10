using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces
{
    public interface IAuditRepository
    {
        void Add(AuditLog log);
    }
}
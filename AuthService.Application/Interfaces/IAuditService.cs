namespace AuthService.Application.Interfaces
{
    public interface IAuditService
    {
        void Log(int? userId, string action, string? description, string ipAddress);
        void DeleteAllForUser(int userId);
    }
}
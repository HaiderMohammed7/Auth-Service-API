using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Interfaces
{
    public interface IAuditService
    {
        void Log(int? userId, string action, string? description, string ipAddress);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{
    public class AuditLog
    {
        public int AuditLogID { get; set; }
        public int? UserID { get; set; }
        public User? User { get; set; }

        public string Action { get; set; } = null!;
        public string? Description { get; set; }
        public string IPAddress { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{
    public class UserRole
    {
        public int UserRoleID { get; set; }

        public int UserID { get; set; }
        public User User { get; set; } = null!;

        public int RoleID { get; set; }
        public Role Role { get; set; } = null!;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Domain.Entities
{
    public class LoginAttempt
    {
        public int LoginAttemptID { get; set; }
        public string Email { get; set; } = null!;
        public string IPAddress { get; set; } = null!;
        public DateTime AttemptedAt { get; set; }
        public bool IsSuccess { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Shared.DTOs
{
    public class ResetPasswordRequestDto
    {
        public string Token { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Shared.DTOs
{
    public class ForgotPasswordRequestDto
    {
        public string Email { get; set; } = null!;
    }
}
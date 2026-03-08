using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        public bool RequiresTwoFactor { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}

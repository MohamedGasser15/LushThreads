using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Auth
{
    public class RegisterResponseDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Auth
{
    public class VerifyResetCodeResponseDto
    {
        public string Email { get; set; }
        public bool IsValid { get; set; }
    }
}

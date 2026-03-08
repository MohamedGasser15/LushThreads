using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Auth
{
    public class TwoFactorVerifyDto
    {
        [Required] public string UserId { get; set; }
        [Required] public string Code { get; set; }
        public bool RememberMe { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Auth
{
    public class VerifyEmailRequestDto
    {
        [Required] public string UserId { get; set; }
        [Required] public string Code { get; set; }
    }
}

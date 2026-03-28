using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Auth
{
    public class VerifyResetCodeRequestDto
    {
        [Required, EmailAddress] public string Email { get; set; }
        [Required] public string Code { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required, MinLength(3)] public string Name { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        [Required, MinLength(6)] public string Password { get; set; }
        [Required, Compare("Password")] public string ConfirmPassword { get; set; }
    }
}

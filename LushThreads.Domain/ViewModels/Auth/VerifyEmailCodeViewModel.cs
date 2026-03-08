using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Domain.ViewModels.Auth
{
    public class VerifyEmailCodeViewModel
    {
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Verification Code")]
        public string Code { get; set; }
        public string Email { get; set; }

    }
}

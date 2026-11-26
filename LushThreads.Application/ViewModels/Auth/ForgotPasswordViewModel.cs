using System.ComponentModel.DataAnnotations;

namespace LushThreads.Application.ViewModels.Auth
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

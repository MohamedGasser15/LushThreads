using System.ComponentModel.DataAnnotations;

namespace LushThreads.Domain.ViewModels.Auth
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}

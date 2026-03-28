using System.ComponentModel.DataAnnotations;

namespace LushThreads.Application.DTOs.User
{
    public class UpdateUserDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? RoleId { get; set; }

        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? StreetAddress { get; set; }
        public string? StreetAddress2 { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? Currency { get; set; }
        public string? PaymentMehtod { get; set; }
        public string? PreferredCarriers { get; set; }
    }
}
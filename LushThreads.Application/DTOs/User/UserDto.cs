using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.User
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? StreetAddress { get; set; }
        public string? StreetAddress2 { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? Currency { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

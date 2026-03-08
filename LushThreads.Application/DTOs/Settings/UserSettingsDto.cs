using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Settings
{
    public class UserSettingsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? Currency { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PreferredCarriers { get; set; }
        public string? StreetAddress { get; set; }
        public string? StreetAddress2 { get; set; }
        public string? SelectedAddress { get; set; }
    }
}

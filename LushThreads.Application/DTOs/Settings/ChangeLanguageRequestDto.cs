using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Settings
{
    public class ChangeLanguageRequestDto
    {
        [Required]
        public string PreferredLanguage { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Settings
{
    public class ChangeCurrencyRequestDto
    {
        [Required]
        public string Currency { get; set; }
    }
}

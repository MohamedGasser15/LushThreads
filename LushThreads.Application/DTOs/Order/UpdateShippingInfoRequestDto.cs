using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Order
{
    public class UpdateShippingInfoRequestDto
    {
        [Required]
        public int OrderId { get; set; }
        [Required]
        public string StreetAddress { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string PostalCode { get; set; }
    }
}

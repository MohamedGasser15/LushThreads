using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Product
{
    public class StockItemDto
    {
        [Required]
        public string Size { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}

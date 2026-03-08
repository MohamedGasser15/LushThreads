using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Order
{
    public class OrderDetailsResponseDto
    {
        public OrderHeaderDto OrderHeader { get; set; }
        public IEnumerable<OrderDetailDto> OrderDetails { get; set; }
    }
}

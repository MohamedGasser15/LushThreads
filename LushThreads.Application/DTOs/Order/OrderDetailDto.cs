using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.DTOs.Order
{
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }        // from Product
        public string ProductImage { get; set; }       // from Product.imgUrl
        public int Count { get; set; }
        public string Size { get; set; }
        public double Price { get; set; }
    }
}

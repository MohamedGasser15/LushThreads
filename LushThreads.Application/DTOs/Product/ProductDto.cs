using System;
using System.Collections.Generic;

namespace LushThreads.Application.DTOs.Product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int Rating { get; set; }
        public string Color { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsNewArrival { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }

        public List<StockDto> Stocks { get; set; }
    }
}
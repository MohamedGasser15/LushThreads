using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LushThreads.Application.DTOs.Product
{
    public class CreateProductRequestDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public string Color { get; set; }

        public int Rating { get; set; }

        public bool IsFeatured { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int BrandId { get; set; }

        public IFormFile? ImageFile { get; set; }
        public string? CroppedImageData { get; set; }

        public List<StockItemDto> Stocks { get; set; } = new();
    }
}
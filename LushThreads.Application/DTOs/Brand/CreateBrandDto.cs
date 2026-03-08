using System.ComponentModel.DataAnnotations;

namespace LushThreads.Application.DTOs.Brand
{
    public class CreateBrandDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
    }
}
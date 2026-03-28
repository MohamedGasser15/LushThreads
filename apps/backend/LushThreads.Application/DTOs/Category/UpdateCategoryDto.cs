using System.ComponentModel.DataAnnotations;

namespace LushThreads.Application.DTOs.Category
{
    public class UpdateCategoryDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; }
    }
}
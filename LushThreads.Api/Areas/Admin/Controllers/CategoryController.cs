using AutoMapper;
using LushThreads.Application.DTOs.Category;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LushThreads.Api.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = SD.Admin)]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public CategoryController(ICategoryService categoryService, IMapper mapper)
        {
            _categoryService = categoryService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all categories.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var categoryDtos = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return Ok(categoryDtos);
        }

        /// <summary>
        /// Retrieves a specific category by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound(new { message = $"Category with ID {id} not found." });

            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Ok(categoryDto);
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User identifier not found in token." });

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var category = _mapper.Map<Category>(createDto);
                await _categoryService.CreateCategoryAsync(category, userId, ipAddress);

                var categoryDto = _mapper.Map<CategoryDto>(category);
                return CreatedAtAction(nameof(GetCategory), new { id = categoryDto.Id }, categoryDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the category.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing category.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateDto)
        {
            if (id != updateDto.Id)
                return BadRequest(new { message = "ID in URL does not match ID in body." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User identifier not found in token." });

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var category = _mapper.Map<Category>(updateDto);
                await _categoryService.UpdateCategoryAsync(category, userId, ipAddress);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the category.", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a category by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User identifier not found in token." });

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                await _categoryService.DeleteCategoryAsync(id, userId, ipAddress);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the category.", error = ex.Message });
            }
        }
    }
}
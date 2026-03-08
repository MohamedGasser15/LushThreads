using AutoMapper;
using LushThreads.Application.DTOs.Brand;
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
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;
        private readonly IMapper _mapper;

        public BrandController(IBrandService brandService, IMapper mapper)
        {
            _brandService = brandService;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all brands.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
        {
            var brands = await _brandService.GetAllBrandsAsync();
            var brandDtos = _mapper.Map<IEnumerable<BrandDto>>(brands);
            return Ok(brandDtos);
        }

        /// <summary>
        /// Retrieves a specific brand by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BrandDto>> GetBrand(int id)
        {
            var brand = await _brandService.GetBrandByIdAsync(id);
            if (brand == null)
                return NotFound(new { message = $"Brand with ID {id} not found." });

            var brandDto = _mapper.Map<BrandDto>(brand);
            return Ok(brandDto);
        }

        /// <summary>
        /// Creates a new brand.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BrandDto>> CreateBrand([FromBody] CreateBrandDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User identifier not found in token." });

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                // Map DTO to Entity
                var brand = _mapper.Map<Brand>(createDto);

                await _brandService.CreateBrandAsync(brand, userId, ipAddress);

                // Map back to DTO to return (with generated Id)
                var brandDto = _mapper.Map<BrandDto>(brand);
                return CreatedAtAction(nameof(GetBrand), new { id = brandDto.Id }, brandDto);
            }
            catch (Exception ex)
            {
                // Log exception as needed
                return StatusCode(500, new { message = "An error occurred while creating the brand.", error = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing brand.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBrand(int id, [FromBody] UpdateBrandDto updateDto)
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

                // Map DTO to Entity
                var brand = _mapper.Map<Brand>(updateDto);

                await _brandService.UpdateBrandAsync(brand, userId, ipAddress);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the brand.", error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a brand by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User identifier not found in token." });

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                await _brandService.DeleteBrandAsync(id, userId, ipAddress);

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the brand.", error = ex.Message });
            }
        }
    }
}
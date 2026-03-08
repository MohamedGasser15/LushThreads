using AutoMapper;
using LushThreads.Application.DTOs.Product;
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Constants;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LushThreads.Api.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = SD.Admin)]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public ProductController(
            IProductService productService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper)
        {
            _productService = productService;
            _userManager = userManager;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all products with related data.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);
            return Ok(productDtos);
        }

        /// <summary>
        /// Get a specific product by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new { message = $"Product with ID {id} not found." });

            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }

        /// <summary>
        /// Create a new product.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductUpsertResponseDto>> CreateProduct([FromForm] CreateProductRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Map DTO to ProductViewModel
            var viewModel = _mapper.Map<ProductViewModel>(request);

            try
            {
                var productId = await _productService.UpsertProductAsync(
                    viewModel,
                    request.ImageFile,
                    request.CroppedImageData,
                    userId,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                return CreatedAtAction(nameof(GetProduct), new { id = productId }, new ProductUpsertResponseDto
                {
                    Id = productId,
                    Message = "Product created successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing product.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductUpsertResponseDto>> UpdateProduct(int id, [FromForm] UpdateProductRequestDto request)
        {
            if (id != request.Id)
                return BadRequest(new { message = "ID in URL does not match ID in body." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var viewModel = _mapper.Map<ProductViewModel>(request);

            try
            {
                var productId = await _productService.UpsertProductAsync(
                    viewModel,
                    request.ImageFile,
                    request.CroppedImageData,
                    userId,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                return Ok(new ProductUpsertResponseDto
                {
                    Id = productId,
                    Message = "Product updated successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a product by ID.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await _productService.DeleteProductAsync(
                    id,
                    userId,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                return Ok(new { message = "Product deleted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        /// <summary>
        /// Toggle featured status of a product.
        /// </summary>
        [HttpPost("{id}/toggle-featured")]
        public async Task<IActionResult> ToggleFeatured(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                var isFeatured = await _productService.ToggleFeaturedAsync(
                    id,
                    userId,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                return Ok(new
                {
                    message = isFeatured ? "Product added to featured items." : "Product removed from featured items.",
                    isFeatured = isFeatured
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }
    }
}
using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Products;
using LushThreads.Infrastructure.Persistence.IRepository;
using LushThreads.Infrastructure.Persistence.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Service class responsible for business logic related to Product entities.
    /// Implements <see cref="IProductService"/>.
    /// </summary>
    public class ProductService : IProductService
    {
        #region Fields

        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IStockRepository _stockRepository;
        private readonly IAdminActivityService _adminActivityService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProductService> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductService"/> class.
        /// </summary>
        /// <param name="productRepository">Repository for Product operations.</param>
        /// <param name="brandRepository">Repository for Brand operations.</param>
        /// <param name="categoryRepository">Repository for Category operations.</param>
        /// <param name="stockRepository">Repository for Stock operations.</param>
        /// <param name="adminActivityRepository">Repository for AdminActivity logging.</param>
        /// <param name="webHostEnvironment">Web host environment for image path.</param>
        /// <param name="logger">Logger instance.</param>
        public ProductService(
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IStockRepository stockRepository,
            IAdminActivityService adminActivityService,
            IWebHostEnvironment webHostEnvironment,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _stockRepository = stockRepository;
            _adminActivityService = adminActivityService;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Saves an image from file or cropped data and returns the image URL.
        /// For new products, an image is required; for updates, if no new image is provided, the existing URL is kept.
        /// </summary>
        /// <param name="file">Uploaded image file.</param>
        /// <param name="croppedImageData">Cropped image base64 data.</param>
        /// <param name="existingImageUrl">Existing image URL (for updates).</param>
        /// <param name="isNew">Indicates whether this is a new product.</param>
        /// <returns>The image URL.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no image is provided for a new product.</exception>
        private async Task<string> SaveImageAsync(IFormFile? file, string? croppedImageData, string? existingImageUrl, bool isNew)
        {
            // If no new image is provided
            if (file == null && string.IsNullOrEmpty(croppedImageData))
            {
                if (isNew)
                {
                    _logger.LogError("Attempted to create a new product without an image.");
                    throw new InvalidOperationException("Product image is required for new products.");
                }
                // For updates, keep the existing image
                return existingImageUrl;
            }

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string productPath = Path.Combine(wwwRootPath, @"img", @"products");

            // Ensure directory exists
            if (!Directory.Exists(productPath))
                Directory.CreateDirectory(productPath);

            // Delete old image if exists (only for updates)
            if (!isNew && !string.IsNullOrEmpty(existingImageUrl))
            {
                var oldImagePath = Path.Combine(wwwRootPath, existingImageUrl.TrimStart('\\'));
                if (File.Exists(oldImagePath))
                {
                    File.Delete(oldImagePath);
                    _logger.LogDebug("Deleted old image: {OldImagePath}", oldImagePath);
                }
            }

            string fileName;
            if (!string.IsNullOrEmpty(croppedImageData))
            {
                // Handle cropped image (base64)
                var base64Data = croppedImageData.Split(',')[1];
                var bytes = Convert.FromBase64String(base64Data);
                string extension = croppedImageData.StartsWith("data:image/jpeg") ? ".jpg" : ".png";
                fileName = Guid.NewGuid().ToString() + extension;
                await File.WriteAllBytesAsync(Path.Combine(productPath, fileName), bytes);
                _logger.LogDebug("Saved cropped image: {FileName}", fileName);
            }
            else if (file != null)
            {
                // Handle uploaded file
                fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
                _logger.LogDebug("Saved uploaded image: {FileName}", fileName);
            }
            else
            {
                // This case should not happen due to the initial check, but just in case
                throw new InvalidOperationException("Unexpected error: no image source available.");
            }

            return @"\img\products\" + fileName;
        }

        /// <summary>
        /// Builds category list with hierarchical display.
        /// </summary>
        private async Task<List<SelectListItem>> BuildCategoryListAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var categoryLookup = categories.ToDictionary(c => c.Category_Id, c => c.Category_Name);

            return categories.Select(i => new SelectListItem
            {
                Text = i.ParentCategoryId == null
                    ? i.Category_Name
                    : $"-- {(categoryLookup.TryGetValue(i.ParentCategoryId.Value, out var parentName) ? parentName : "Unknown")} > {i.Category_Name}",
                Value = i.Category_Id.ToString()
            }).ToList();
        }

        /// <summary>
        /// Fixes legacy products with default DateAdded (from original logic).
        /// </summary>
        private async Task FixLegacyProductsAsync()
        {
            var legacyProducts = await _productRepository.GetAllAsync(p => p.DateAdded == DateTime.MinValue);
            if (legacyProducts.Any())
            {
                var referenceDate = DateTime.Now.AddDays(-60);
                foreach (var product in legacyProducts)
                {
                    product.DateAdded = referenceDate;
                    await _productRepository.UpdateAsync(product);
                }
                _logger.LogInformation("Fixed {Count} legacy products with default DateAdded.", legacyProducts.Count);
            }
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            _logger.LogDebug("Retrieving all products with related data.");
            var products = await _productRepository.GetAllAsync(
                includeProperties: "Stocks,Category,Brand",
                orderBy: q => q.OrderByDescending(p => p.Product_Id)
            );
            _logger.LogDebug("Retrieved {Count} products.", products.Count);
            return products;
        }

        /// <inheritdoc />
        public async Task<Product?> GetProductByIdAsync(int id)
        {
            _logger.LogDebug("Retrieving product with ID {ProductId}.", id);
            var product = await _productRepository.GetAsync(
                filter: p => p.Product_Id == id,
                includeProperties: "Stocks,Category,Brand"
            );
            if (product == null)
                _logger.LogWarning("Product with ID {ProductId} not found.", id);
            return product;
        }

        /// <inheritdoc />
        public async Task<ProductViewModel> GetProductViewModelForUpsertAsync(int id)
        {
            _logger.LogDebug("Preparing ProductViewModel for upsert, ID: {ProductId}.", id);

            var viewModel = new ProductViewModel();

            // Populate brand list
            var brands = await _brandRepository.GetAllAsync();
            viewModel.BrandList = brands.Select(b => new SelectListItem
            {
                Text = b.Brand_Name,
                Value = b.Brand_Id.ToString()
            }).ToList();

            // Populate category list (hierarchical)
            viewModel.CategoryList = await BuildCategoryListAsync();

            if (id == 0)
            {
                viewModel.Product = new Product();
                viewModel.Stocks = new List<Stock>();
            }
            else
            {
                viewModel.Product = await GetProductByIdAsync(id);
                if (viewModel.Product == null)
                    return null; // Will be handled by controller
                viewModel.Stocks = viewModel.Product.Stocks?.ToList() ?? new List<Stock>();
            }

            return viewModel;
        }

        /// <inheritdoc />
        public async Task<int> UpsertProductAsync(ProductViewModel viewModel, IFormFile? file, string? croppedImageData, string userId, string? ipAddress)
        {
            try
            {
                _logger.LogInformation("Starting upsert of product '{ProductName}' by user {UserId}.", viewModel.Product?.Product_Name, userId);

                bool isNew = viewModel.Product.Product_Id == 0;

                // Validate category is not a parent category (must be subcategory)
                var selectedCategory = await _categoryRepository.GetAsync(c => c.Category_Id == viewModel.Product.Category_Id);
                if (selectedCategory != null && selectedCategory.ParentCategoryId == null)
                {
                    throw new InvalidOperationException("Products must be assigned to a subcategory, not a parent category.");
                }

                // Validate image for new products
                if (isNew && file == null && string.IsNullOrEmpty(croppedImageData))
                {
                    _logger.LogWarning("Attempt to create new product without image.");
                    throw new InvalidOperationException("Product image is required.");
                }

                string imageUrl;

                if (isNew)
                {
                    // For new product: no existing image
                    imageUrl = await SaveImageAsync(file, croppedImageData, null, isNew);
                    viewModel.Product.imgUrl = imageUrl;
                    viewModel.Product.DateAdded = DateTime.Now;
                    await _productRepository.CreateAsync(viewModel.Product);
                    await _adminActivityService.LogActivityAsync(userId, "AddProduct", $"Add Product (Id: {viewModel.Product.Product_Id})", ipAddress);
                    _logger.LogInformation("Product created with ID {ProductId}.", viewModel.Product.Product_Id);
                }
                else
                {
                    // For update: fetch existing product to get current image URL and preserve fields
                    var existingProduct = await _productRepository.GetAsync(p => p.Product_Id == viewModel.Product.Product_Id);
                    if (existingProduct == null)
                    {
                        _logger.LogWarning("Product with ID {ProductId} not found for update.", viewModel.Product.Product_Id);
                        throw new InvalidOperationException($"Product with id {viewModel.Product.Product_Id} not found.");
                    }

                    // Handle image: pass existing image URL
                    imageUrl = await SaveImageAsync(file, croppedImageData, existingProduct.imgUrl, isNew);

                    // Update properties from viewModel (only those that are allowed to change)
                    existingProduct.Product_Name = viewModel.Product.Product_Name;
                    existingProduct.Product_Description = viewModel.Product.Product_Description;
                    existingProduct.Product_Price = viewModel.Product.Product_Price;
                    existingProduct.Product_Color = viewModel.Product.Product_Color;
                    existingProduct.IsFeatured = viewModel.Product.IsFeatured;
                    existingProduct.Category_Id = viewModel.Product.Category_Id;
                    existingProduct.brand_Id = viewModel.Product.brand_Id;
                    existingProduct.Product_Rating = viewModel.Product.Product_Rating; // if present in form
                    existingProduct.imgUrl = imageUrl;
                    // Do NOT update DateAdded or NewArrivalDurationDays

                    await _productRepository.UpdateAsync(existingProduct);
                    await _adminActivityService.LogActivityAsync(userId, "UpdateProduct", $"Update Product (Id: {existingProduct.Product_Id})", ipAddress);
                    _logger.LogInformation("Product ID {ProductId} updated.", existingProduct.Product_Id);

                    // Replace viewModel.Product with updated entity for further use (e.g., stocks)
                    viewModel.Product = existingProduct;
                }

                // Handle stocks: delete old and add new
                if (!isNew)
                {
                    var existingStocks = await _stockRepository.GetAllAsync(s => s.Product_Id == viewModel.Product.Product_Id);
                    if (existingStocks.Any())
                        await _stockRepository.DeleteRangeAsync(existingStocks);
                }

                if (viewModel.Stocks != null && viewModel.Stocks.Any())
                {
                    foreach (var stock in viewModel.Stocks)
                    {
                        stock.Product_Id = viewModel.Product.Product_Id;
                        await _stockRepository.CreateAsync(stock);
                    }
                }

                // Handle legacy products (from original controller logic)
                if (!isNew)
                {
                    await FixLegacyProductsAsync();
                }

                return viewModel.Product.Product_Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during product upsert.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DeleteProductAsync(int id, string userId, string? ipAddress)
        {
            try
            {
                _logger.LogInformation("Starting deletion of product ID {ProductId} by user {UserId}.", id, userId);

                var product = await _productRepository.GetAsync(p => p.Product_Id == id, includeProperties: "Stocks");
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found for deletion.", id);
                    throw new InvalidOperationException($"Product with id {id} not found.");
                }

                // Delete associated stocks
                if (product.Stocks != null && product.Stocks.Any())
                {
                    await _stockRepository.DeleteRangeAsync(product.Stocks);
                }

                // Delete image file
                if (!string.IsNullOrEmpty(product.imgUrl))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.imgUrl.TrimStart('\\'));
                    if (File.Exists(imagePath))
                        File.Delete(imagePath);
                }

                // Delete product
                await _productRepository.DeleteAsync(product);

                // Log activity
                await _adminActivityService.LogActivityAsync(userId, "DeleteProduct", $"Delete Product (Id: {id})", ipAddress);

                _logger.LogInformation("Product ID {ProductId} deleted successfully.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product ID {ProductId}.", id);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> ToggleFeaturedAsync(int id, string userId, string? ipAddress)
        {
            try
            {
                _logger.LogInformation("Toggling featured status for product ID {ProductId} by user {UserId}.", id, userId);

                var product = await _productRepository.GetAsync(p => p.Product_Id == id);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found for featured toggle.", id);
                    throw new InvalidOperationException($"Product with id {id} not found.");
                }

                product.IsFeatured = !product.IsFeatured;
                await _productRepository.UpdateAsync(product);

                string activityType = product.IsFeatured ? "AddToFeatured" : "RemoveFromFeatured";
                string description = $"{(product.IsFeatured ? "Add" : "Remove")} Product (Id: {id}) to/from featured items";
                await _adminActivityService.LogActivityAsync(userId, activityType, description, ipAddress);

                _logger.LogInformation("Product ID {ProductId} featured status set to {IsFeatured}.", id, product.IsFeatured);

                return product.IsFeatured;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling featured for product ID {ProductId}.", id);
                throw;
            }
        }

        #endregion
    }
}
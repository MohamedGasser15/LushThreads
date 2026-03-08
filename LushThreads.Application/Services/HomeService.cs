using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Home;
using LushThreads.Infrastructure.Persistence.IRepository;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    /// <summary>
    /// Service for home page and shop operations.
    /// Implements <see cref="IHomeService"/>.
    /// </summary>
    public class HomeService : IHomeService
    {
        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Brand> _brandRepository;
        private readonly ILogger<HomeService> _logger;

        #endregion

        #region Constructor

        public HomeService(
            IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IRepository<Brand> brandRepository,
            ILogger<HomeService> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _logger = logger;
        }

        #endregion

        #region Private Helpers

        private async Task<List<int>> GetSubCategoryIdsAsync(int parentCategoryId)
        {
            var subCategories = await _categoryRepository.GetAllAsync(
                filter: c => c.ParentCategoryId == parentCategoryId,
                isTracking: false
            );
            return subCategories.Select(c => c.Category_Id).ToList();
        }

        private HomeViewModel MapProductToHomeViewModel(Product p)
        {
            return new HomeViewModel
            {
                Product_Id = p.Product_Id,
                Product_Name = p.Product_Name,
                imgUrl = p.imgUrl,
                BrandName = p.Brand?.Brand_Name ?? "Unknown",
                IsFeatured = p.IsFeatured,
                DateAdded = p.DateAdded,
                Product_Rating = p.Product_Rating,
                Product_Price = p.Product_Price,
                AvailableSizes = p.Stocks?
                    .Where(s => s.Quantity > 0)
                    .Select(s => s.Size)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList() ?? new List<string>()
            };
        }

        private object MapProductToAnonymous(Product p)
        {
            return new
            {
                ProductId = p.Product_Id,
                ProductName = p.Product_Name,
                ImgUrl = p.imgUrl,
                BrandName = p.Brand?.Brand_Name ?? "Unknown",
                CategoryName = p.Category?.Category_Name ?? "Unknown",
                IsFeatured = p.IsFeatured,
                ProductRating = p.Product_Rating,
                ProductPrice = p.Product_Price,
                AvailableSizes = p.Stocks?
                    .Where(s => s.Quantity > 0)
                    .Select(s => s.Size)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList() ?? new List<string>()
            };
        }

        #endregion

        #region Home Page

        public async Task<HomeViewModel> GetHomeViewModelAsync(int? categoryId)
        {
            _logger.LogDebug("Getting home view model with category {CategoryId}.", categoryId);

            List<int> subCategoryIds = null;
            if (categoryId.HasValue && categoryId > 0)
            {
                subCategoryIds = await GetSubCategoryIdsAsync(categoryId.Value);
            }

            // Featured products filter
            Expression<Func<Product, bool>> featuredFilter;
            if (categoryId.HasValue && categoryId > 0)
            {
                if (subCategoryIds.Any())
                {
                    featuredFilter = p => subCategoryIds.Contains(p.Category_Id);
                }
                else
                {
                    featuredFilter = p => p.Category_Id == categoryId.Value;
                }
            }
            else
            {
                featuredFilter = p => p.IsFeatured;
            }

            var featuredProducts = await _productRepository.GetAllAsync(
                filter: featuredFilter,
                includeProperties: "Brand,Category,Stocks",
                orderBy: q => q.OrderBy(p => p.Product_Id),
                take: 8,
                isTracking: false
            );

            // New arrivals
            var newArrivals = await _productRepository.GetAllAsync(
                includeProperties: "Brand,Category,Stocks",
                orderBy: q => q.OrderByDescending(p => p.Product_Id),
                take: 8,
                isTracking: false
            );

            return new HomeViewModel
            {
                FeaturedProducts = featuredProducts.Select(p => MapProductToHomeViewModel(p)).ToList(),
                NewArrivals = newArrivals.Select(p => MapProductToHomeViewModel(p)).ToList()
            };
        }

        public async Task<object> GetHomeProductsJsonAsync(int? categoryId)
        {
            _logger.LogDebug("Getting home products JSON for category {CategoryId}.", categoryId);

            List<int> subCategoryIds = null;
            if (categoryId.HasValue && categoryId > 0)
            {
                subCategoryIds = await GetSubCategoryIdsAsync(categoryId.Value);
            }

            Expression<Func<Product, bool>> filter;
            if (categoryId.HasValue && categoryId > 0)
            {
                if (subCategoryIds.Any())
                {
                    filter = p => subCategoryIds.Contains(p.Category_Id);
                }
                else
                {
                    filter = p => p.Category_Id == categoryId.Value;
                }
            }
            else
            {
                filter = p => p.IsFeatured;
            }

            var products = await _productRepository.GetAllAsync(
                filter: filter,
                includeProperties: "Brand,Category,Stocks",
                orderBy: q => q.OrderBy(p => p.Product_Id),
                take: 8,
                isTracking: false
            );

            var result = products.Select(p => MapProductToAnonymous(p)).ToList();
            return new { products = result, currentCategoryId = categoryId };
        }

        #endregion

        #region Shop

        public async Task<(IEnumerable<ShopProductViewModel> Products, int TotalPages, int TotalProducts)> GetShopProductsAsync(int? categoryId, int page, int pageSize)
        {
            _logger.LogDebug("Getting shop products page {Page}, size {PageSize}, category {CategoryId}.", page, pageSize, categoryId);

            List<int> subCategoryIds = null;
            if (categoryId.HasValue && categoryId > 0)
            {
                subCategoryIds = await GetSubCategoryIdsAsync(categoryId.Value);
            }

            Expression<Func<Product, bool>> filter = null;
            if (categoryId.HasValue && categoryId > 0)
            {
                if (subCategoryIds.Any())
                {
                    filter = p => subCategoryIds.Contains(p.Category_Id);
                }
                else
                {
                    filter = p => p.Category_Id == categoryId.Value;
                }
            }

            // Count total products matching filter
            var totalProducts = await _productRepository.GetAllAsync(
                filter: filter,
                isTracking: false
            );
            int totalCount = totalProducts.Count;
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            page = Math.Min(page, Math.Max(1, totalPages));

            // Get paginated products
            var products = await _productRepository.GetAllAsync(
                filter: filter,
                includeProperties: "Brand,Category,Stocks",
                orderBy: q => q.OrderBy(p => p.Product_Id),
                skip: (page - 1) * pageSize,
                take: pageSize,
                isTracking: false
            );

            var result = products.Select(p => new ShopProductViewModel
            {
                ProductId = p.Product_Id,
                ProductName = p.Product_Name,
                ImgUrl = p.imgUrl,
                BrandName = p.Brand?.Brand_Name ?? "Unknown",
                CategoryName = p.Category?.Category_Name ?? "Unknown",
                IsFeatured = p.IsFeatured,
                ProductRating = p.Product_Rating,
                ProductPrice = p.Product_Price,
                AvailableSizes = p.Stocks?
                    .Where(s => s.Quantity > 0)
                    .Select(s => s.Size)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList() ?? new List<string>()
            }).ToList();

            return (result, totalPages, totalCount);
        }
        #endregion

        #region Product Details

        public async Task<Product?> GetProductDetailsAsync(int productId)
        {
            _logger.LogDebug("Getting product details for ID {ProductId}.", productId);
            return await _productRepository.GetAsync(
                filter: p => p.Product_Id == productId,
                includeProperties: "Category,Brand,Stocks",
                isTracking: false
            );
        }

        public async Task<object?> GetProductDetailsJsonAsync(int productId)
        {
            _logger.LogDebug("Getting product JSON for ID {ProductId}.", productId);

            var product = await _productRepository.GetAsync(
                filter: p => p.Product_Id == productId,
                includeProperties: "Brand,Category,Stocks",
                isTracking: false
            );

            if (product == null)
                return null;

            return new
            {
                productId = product.Product_Id,
                productName = product.Product_Name,
                imgUrl = product.imgUrl,
                productRating = product.Product_Rating,
                productPrice = product.Product_Price,
                description = product.Product_Description,
                color = product.Product_Color,
                brandName = product.Brand?.Brand_Name,
                categoryName = product.Category?.Category_Name,
                availableSizes = product.Stocks?
                    .Where(s => s.Quantity > 0)
                    .Select(s => s.Size)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList()
            };
        }

        public async Task<List<string>> GetProductSizesAsync(int productId)
        {
            _logger.LogDebug("Getting sizes for product ID {ProductId}.", productId);

            var product = await _productRepository.GetAsync(
                filter: p => p.Product_Id == productId,
                includeProperties: "Stocks",
                isTracking: false
            );

            return product?.Stocks?
                .Where(s => s.Quantity > 0)
                .Select(s => s.Size)
                .ToList() ?? new List<string>();
        }

        #endregion

        #region Category & Brand Filtering

        public async Task<(string CategoryName, List<Product> Products)> GetProductsByCategoryAsync(string categoryUrl)
        {
            _logger.LogDebug("Getting products by category URL: {CategoryUrl}.", categoryUrl);

            // Map URL to category name
            var categoryName = categoryUrl switch
            {
                "mens-fashion" => "Men's Fashion",
                "womens-fashion" => "Women's Fashion",
                "kids-fashion" => "Kids Fashion",
                "footwear" => "Footwear",
                "accessories" => "Accessories",
                "sportswear" => "Sportswear",
                "luxury-collection" => "Luxury Collection",
                "summer-essentials" => "Seasonal Collections",
                _ => null
            };

            if (categoryName == null)
                return (null, null);

            var parentCategory = await _categoryRepository.GetAsync(c => c.Category_Name == categoryName && c.ParentCategoryId == null);
            if (parentCategory == null)
                return (null, null);

            var categoryIds = await _categoryRepository.GetAllAsync(
                filter: c => c.ParentCategoryId == parentCategory.Category_Id || c.Category_Id == parentCategory.Category_Id,
                isTracking: false
            );
            var ids = categoryIds.Select(c => c.Category_Id).ToList();

            var products = await _productRepository.GetAllAsync(
                filter: p => ids.Contains(p.Category_Id),
                includeProperties: "Brand,Category,Stocks",
                isTracking: false
            );

            return (categoryName, products.ToList());
        }

        public async Task<(string CategoryName, List<Product> Products)> GetProductsByChildCategoryAsync(string categoryUrl)
        {
            _logger.LogDebug("Getting products by child category URL: {CategoryUrl}.", categoryUrl);

            var categoryName = categoryUrl.ToLower();

            // Special handling for "shorts" and "hoodie" (they appear in multiple parent categories)
            if (categoryName == "shorts" || categoryName == "hoodie")
            {
                var parentCategoryNames = new[] { "Men's Fashion", "Women's Fashion", "Unisex" };
                var parentCategories = await _categoryRepository.GetAllAsync(
                    filter: c => parentCategoryNames.Contains(c.Category_Name) && c.ParentCategoryId == null,
                    isTracking: false
                );
                var parentIds = parentCategories.Select(c => c.Category_Id).ToList();

                var childCategories = await _categoryRepository.GetAllAsync(
                    filter: c => c.Category_Name.ToLower() == categoryName &&
                                 c.ParentCategoryId.HasValue &&
                                 parentIds.Contains(c.ParentCategoryId.Value),
                    isTracking: false
                );
                var childIds = childCategories.Select(c => c.Category_Id).ToList();

                if (!childIds.Any())
                    return (null, null);

                var products = await _productRepository.GetAllAsync(
                    filter: p => childIds.Contains(p.Category_Id),
                    includeProperties: "Brand,Category,Stocks",
                    isTracking: false
                );

                var displayName = categoryName == "shorts" ? "Shorts" : "Hoodie";
                return (displayName, products.ToList());
            }

            var childCategory = await _categoryRepository.GetAsync(
                filter: c => c.Category_Name.ToLower() == categoryName && c.ParentCategoryId != null,
                isTracking: false
            );

            if (childCategory == null)
                return (null, null);

            var productsForChild = await _productRepository.GetAllAsync(
                filter: p => p.Category_Id == childCategory.Category_Id,
                includeProperties: "Brand,Category,Stocks",
                isTracking: false
            );

            return (childCategory.Category_Name, productsForChild.ToList());
        }

        public async Task<(string BrandName, List<Product> Products)> GetProductsByBrandAsync(string brandUrl)
        {
            _logger.LogDebug("Getting products by brand URL: {BrandUrl}.", brandUrl);

            var brandLower = brandUrl.ToLower();
            bool isLocalBrands = brandLower == "local";

            string brandName = null;
            if (!isLocalBrands)
            {
                brandName = brandLower switch
                {
                    "nike" => "Nike",
                    "adidas" => "Adidas",
                    "puma" => "Puma",
                    "zara" => "Zara",
                    "hm" => "H&M",
                    "levis" => "Levi's",
                    "nightbird" => "NightBird",
                    _ => null
                };

                if (brandName == null)
                    return (null, null);
            }

            List<Product> products;
            if (isLocalBrands)
            {
                var excludedBrands = new[] { "Nike", "Adidas", "Puma", "Zara", "H&M", "Levi's", "NightBird" };
                products = await _productRepository.GetAllAsync(
                    filter: p => p.Brand != null && !excludedBrands.Contains(p.Brand.Brand_Name),
                    includeProperties: "Brand,Category,Stocks",
                    isTracking: false
                );
                brandName = "Local Brands";
            }
            else
            {
                products = await _productRepository.GetAllAsync(
                    filter: p => p.Brand.Brand_Name == brandName,
                    includeProperties: "Brand,Category,Stocks",
                    isTracking: false
                );
            }

            return (brandName, products.ToList());
        }

        #endregion

        #region Search

        public async Task<List<Product>> SearchProductsAsync(string searchTerm)
        {
            _logger.LogDebug("Searching products with term: {SearchTerm}.", searchTerm);

            if (string.IsNullOrEmpty(searchTerm))
                return new List<Product>();

            return await _productRepository.GetAllAsync(
                filter: p => p.Product_Name.Contains(searchTerm) || p.Product_Description.Contains(searchTerm),
                includeProperties: "Brand",
                isTracking: false
            );
        }

        #endregion
    }
}
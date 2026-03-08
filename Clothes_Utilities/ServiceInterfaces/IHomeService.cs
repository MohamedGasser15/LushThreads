using LushThreads.Domain.Entites;
using LushThreads.Domain.ViewModels.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.ServiceInterfaces
{
    /// <summary>
    /// Service interface for home page and shop operations.
    /// </summary>
    public interface IHomeService
    {
        #region Home Page

        /// <summary>
        /// Gets featured products and new arrivals for home page.
        /// </summary>
        Task<HomeViewModel> GetHomeViewModelAsync(int? categoryId);

        /// <summary>
        /// Gets products for AJAX request in home page.
        /// </summary>
        Task<object> GetHomeProductsJsonAsync(int? categoryId);

        #endregion

        #region Shop

        /// <summary>
        /// Gets paginated products for shop page.
        /// </summary>
        Task<(IEnumerable<ShopProductViewModel> Products, int TotalPages, int TotalProducts)> GetShopProductsAsync(int? categoryId, int page, int pageSize);
        #endregion

        #region Product Details

        /// <summary>
        /// Gets a product by ID with related data.
        /// </summary>
        Task<Product?> GetProductDetailsAsync(int productId);

        /// <summary>
        /// Gets product details for JSON response.
        /// </summary>
        Task<object?> GetProductDetailsJsonAsync(int productId);

        /// <summary>
        /// Gets available sizes for a product.
        /// </summary>
        Task<List<string>> GetProductSizesAsync(int productId);

        #endregion

        #region Category & Brand Filtering

        /// <summary>
        /// Gets products by main category.
        /// </summary>
        Task<(string CategoryName, List<Product> Products)> GetProductsByCategoryAsync(string categoryUrl);

        /// <summary>
        /// Gets products by child category.
        /// </summary>
        Task<(string CategoryName, List<Product> Products)> GetProductsByChildCategoryAsync(string categoryUrl);

        /// <summary>
        /// Gets products by brand.
        /// </summary>
        Task<(string BrandName, List<Product> Products)> GetProductsByBrandAsync(string brandUrl);

        #endregion

        #region Search

        /// <summary>
        /// Searches products by term.
        /// </summary>
        Task<List<Product>> SearchProductsAsync(string searchTerm);

        #endregion
    }
}
